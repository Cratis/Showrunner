// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Cratis.Showrunner.StageOperator.Building;
using Cratis.Showrunner.StageOperator.Metering;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.StageOperator.Kubernetes;

/// <summary>
/// The <see cref="IKubernetesGateway"/> backed by the Kubernetes API. Objects are applied with server-side apply
/// under the field manager <c>stage-operator</c>, so applying is idempotent and never fights another writer over
/// a field it does not own.
/// </summary>
public class KubernetesGateway : IKubernetesGateway
{
    const string FieldManager = "stage-operator";

    readonly IKubernetes _client;
    readonly ILogger<KubernetesGateway> _logger;

    /// <summary>Initializes a new instance of the <see cref="KubernetesGateway"/> class.</summary>
    /// <param name="client">The Kubernetes client.</param>
    /// <param name="logger">The logger.</param>
    public KubernetesGateway(IKubernetes client, ILogger<KubernetesGateway> logger)
    {
        _client = client;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StageApplication>> ListApplications(string? namespaceName, CancellationToken cancellationToken)
    {
        var result = namespaceName is null
            ? await _client.CustomObjects.ListClusterCustomObjectAsync(StageApplication.Group, StageApplication.Version, StageApplication.Plural, cancellationToken: cancellationToken)
            : await _client.CustomObjects.ListNamespacedCustomObjectAsync(StageApplication.Group, StageApplication.Version, namespaceName, StageApplication.Plural, cancellationToken: cancellationToken);

        return Deserialize<StageApplicationList>(result).Items;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<StageApplication> WatchApplications(string? namespaceName, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var stream = namespaceName is null
            ? _client.CustomObjects.WatchListClusterCustomObjectAsync(StageApplication.Group, StageApplication.Version, StageApplication.Plural, timeoutSeconds: 300, onError: error => _logger.LogWarning(error, "The watch of StageApplication resources failed"), cancellationToken: cancellationToken)
            : _client.CustomObjects.WatchListNamespacedCustomObjectAsync(StageApplication.Group, StageApplication.Version, namespaceName, StageApplication.Plural, timeoutSeconds: 300, onError: error => _logger.LogWarning(error, "The watch of StageApplication resources failed"), cancellationToken: cancellationToken);

        await foreach (var (type, item) in stream.WithCancellation(cancellationToken))
        {
            if (type != WatchEventType.Bookmark && type != WatchEventType.Error)
            {
                yield return Deserialize<StageApplication>(item);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<StageApplication?> GetApplication(string namespaceName, string name, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _client.CustomObjects.GetNamespacedCustomObjectAsync(StageApplication.Group, StageApplication.Version, namespaceName, StageApplication.Plural, name, cancellationToken);
            return Deserialize<StageApplication>(result);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<ObservedState> Observe(StageApplication application, CancellationToken cancellationToken)
    {
        var deployment = await IgnoreNotFound(() => _client.AppsV1.ReadNamespacedDeploymentAsync(application.Name, application.Namespace, cancellationToken: cancellationToken));
        var statefulSet = await IgnoreNotFound(() => _client.AppsV1.ReadNamespacedStatefulSetAsync(ApplicationNames.Mongo(application), application.Namespace, cancellationToken: cancellationToken));
        var claims = await _client.CoreV1.ListNamespacedPersistentVolumeClaimAsync(application.Namespace, labelSelector: MongoSelector(application), cancellationToken: cancellationToken);

        ObservedDeployment? observed = null;
        if (deployment is not null)
        {
            observed = new ObservedDeployment(
                deployment.Spec?.Template?.Spec?.Containers?.FirstOrDefault(c => c.Name == ApplicationNames.StageContainer)?.Image,
                deployment.Status?.Replicas ?? 0,
                deployment.Status?.ReadyReplicas ?? 0,
                deployment.Status?.UpdatedReplicas ?? 0,
                deployment.Metadata?.Generation ?? 0,
                deployment.Status?.ObservedGeneration ?? 0,
                deployment.Status?.Conditions?.Any(c => c.Type == "Progressing" && c.Reason == "ProgressDeadlineExceeded") == true,
                deployment.Spec?.Template?.Spec?.Volumes?.FirstOrDefault(v => v.Name == ApplicationNames.PackageVolume)?.ConfigMap?.Name);
        }

        return new ObservedState(observed, statefulSet is not null || claims.Items.Count > 0);
    }

    /// <inheritdoc/>
    public async Task Apply(DesiredObjects desired, CancellationToken cancellationToken)
    {
        foreach (var item in desired.All)
        {
            await ApplyObject(item, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task StopWorkloads(StageApplication application, CancellationToken cancellationToken)
    {
        var patch = new V1Patch("{\"spec\":{\"replicas\":0}}", V1Patch.PatchType.MergePatch);
        await IgnoreNotFound(() => _client.AppsV1.PatchNamespacedDeploymentScaleAsync(patch, application.Name, application.Namespace, cancellationToken: cancellationToken));
        await IgnoreNotFound(() => _client.AppsV1.PatchNamespacedStatefulSetScaleAsync(patch, ApplicationNames.Mongo(application), application.Namespace, cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task DeleteIngress(StageApplication application, CancellationToken cancellationToken) =>
        await IgnoreNotFound(() => _client.NetworkingV1.DeleteNamespacedIngressAsync(application.Name, application.Namespace, cancellationToken: cancellationToken));

    /// <inheritdoc/>
    public async Task DeleteData(StageApplication application, CancellationToken cancellationToken)
    {
        await IgnoreNotFound(() => _client.AppsV1.DeleteNamespacedStatefulSetAsync(ApplicationNames.Mongo(application), application.Namespace, cancellationToken: cancellationToken));
        await IgnoreNotFound(() => _client.CoreV1.DeleteCollectionNamespacedPersistentVolumeClaimAsync(application.Namespace, labelSelector: MongoSelector(application), cancellationToken: cancellationToken));
        _logger.LogInformation("Removed the data of {Namespace}/{Name}", application.Namespace, application.Name);
    }

    /// <inheritdoc/>
    public Task AddFinalizer(StageApplication application, CancellationToken cancellationToken) =>
        PatchFinalizers(application, [.. application.Metadata.Finalizers ?? [], StageApplication.RetentionFinalizer], cancellationToken);

    /// <inheritdoc/>
    public Task RemoveFinalizer(StageApplication application, CancellationToken cancellationToken) =>
        PatchFinalizers(application, (application.Metadata.Finalizers ?? []).Where(f => f != StageApplication.RetentionFinalizer).ToList(), cancellationToken);

    /// <inheritdoc/>
    public async Task UpdateStatus(StageApplication application, Func<StageApplicationStatus, StageApplicationStatus> change, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 4; attempt++)
        {
            var latest = await GetApplication(application.Namespace, application.Name, cancellationToken);
            if (latest is null)
            {
                return;
            }

            latest.Status = change(latest.Status ?? new StageApplicationStatus());
            try
            {
                await _client.CustomObjects.ReplaceNamespacedCustomObjectStatusAsync(latest, StageApplication.Group, StageApplication.Version, latest.Namespace, StageApplication.Plural, latest.Name, cancellationToken: cancellationToken);
                return;
            }
            catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.Conflict)
            {
                _logger.LogDebug("Status of {Namespace}/{Name} changed while it was written; retrying", application.Namespace, application.Name);
            }
        }

        throw new InvalidOperationException($"The status of {application.Namespace}/{application.Name} kept changing while it was written.");
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContainerUsage>?> GetContainerUsage(StageApplication application, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _client.CustomObjects.ListNamespacedCustomObjectAsync(
                "metrics.k8s.io",
                "v1beta1",
                application.Namespace,
                "pods",
                labelSelector: $"{ApplicationNames.InstanceLabel}={application.Name}",
                cancellationToken: cancellationToken);

            return
            [
                .. Deserialize<PodMetricsList>(result).Items.SelectMany(pod => pod.Containers.Select(container => new ContainerUsage(
                    pod.Metadata.Name,
                    container.Name,
                    QuantityParser.Parse(container.Usage.GetValueOrDefault("cpu")),
                    (long)QuantityParser.Parse(container.Usage.GetValueOrDefault("memory")))))
            ];
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden or HttpStatusCode.ServiceUnavailable)
        {
            _logger.LogDebug("Pod metrics are not available for {Namespace}/{Name}: {Status}", application.Namespace, application.Name, ex.Response.StatusCode);
            return null;
        }
    }

    static string MongoSelector(StageApplication application) =>
        $"{ApplicationNames.InstanceLabel}={application.Name},{ApplicationNames.ComponentLabel}={ApplicationNames.MongoComponent}";

    static T Deserialize<T>(object value) =>
        KubernetesJson.Deserialize<T>(value is JsonElement element ? element.GetRawText() : value.ToString() ?? "{}");

    static async Task<T?> IgnoreNotFound<T>(Func<Task<T>> call)
        where T : class
    {
        try
        {
            return await call();
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    async Task PatchFinalizers(StageApplication application, IList<string> finalizers, CancellationToken cancellationToken)
    {
        var body = KubernetesJson.Serialize(new { metadata = new { resourceVersion = application.Metadata.ResourceVersion, finalizers } });
        await _client.CustomObjects.PatchNamespacedCustomObjectAsync(new V1Patch(body, V1Patch.PatchType.MergePatch), StageApplication.Group, StageApplication.Version, application.Namespace, StageApplication.Plural, application.Name, cancellationToken: cancellationToken);
    }

    async Task ApplyObject(IKubernetesObject<V1ObjectMeta> item, CancellationToken cancellationToken)
    {
        var patch = new V1Patch(KubernetesJson.Serialize(item), V1Patch.PatchType.ApplyPatch);
        var name = item.Metadata.Name;
        var ns = item.Metadata.NamespaceProperty;

        switch (item)
        {
            case V1Deployment:
                await _client.AppsV1.PatchNamespacedDeploymentAsync(patch, name, ns, fieldManager: FieldManager, force: true, cancellationToken: cancellationToken);
                break;
            case V1StatefulSet:
                await _client.AppsV1.PatchNamespacedStatefulSetAsync(patch, name, ns, fieldManager: FieldManager, force: true, cancellationToken: cancellationToken);
                break;
            case V1Service:
                await _client.CoreV1.PatchNamespacedServiceAsync(patch, name, ns, fieldManager: FieldManager, force: true, cancellationToken: cancellationToken);
                break;
            case V1Ingress:
                await _client.NetworkingV1.PatchNamespacedIngressAsync(patch, name, ns, fieldManager: FieldManager, force: true, cancellationToken: cancellationToken);
                break;
            case V1NetworkPolicy:
                await _client.NetworkingV1.PatchNamespacedNetworkPolicyAsync(patch, name, ns, fieldManager: FieldManager, force: true, cancellationToken: cancellationToken);
                break;
            default:
                throw new NotSupportedException($"{item.Kind} is not an object the operator applies.");
        }
    }
}
