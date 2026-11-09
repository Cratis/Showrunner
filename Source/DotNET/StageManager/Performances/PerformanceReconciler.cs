// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Brings a Performance into the cluster, or says why it cannot be. Nothing is applied until everything the Performance needs is in hand.
/// </summary>
/// <param name="cluster">The cluster.</param>
/// <param name="preparation">The preparation that gates the rollout.</param>
/// <param name="time">The time provider.</param>
/// <param name="logger">The logger.</param>
public class PerformanceReconciler(IClusterResources cluster, PerformancePreparation preparation, TimeProvider time, ILogger<PerformanceReconciler> logger)
{
    /// <summary>
    /// Reconciles the Performance and writes its status.
    /// </summary>
    /// <param name="performance">The Performance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The phase the Performance ended in.</returns>
    public async Task<PerformancePhase> ReconcileAsync(Performance performance, CancellationToken cancellationToken)
    {
        performance.Status ??= new();
        var status = new PerformanceStatusBuilder(performance, time);
        var now = time.GetUtcNow();
        if (LifetimeClock.IsExpired(performance.Spec, now))
        {
            await ExpireAsync(performance, status, cancellationToken);
        }
        else
        {
            var prepared = await preparation.PrepareAsync(performance, status, cancellationToken);
            if (prepared is not null)
            {
                await RollOutAsync(performance, prepared, status, cancellationToken);
            }

            UsageAccumulator.Add(performance.Status, await cluster.GetUsageAsync(performance.Metadata.NamespaceProperty, performance.Metadata.Name, cancellationToken), now);
        }

        status.Write();
        await cluster.UpdateStatusAsync(performance, cancellationToken);
        logger.PerformancePhase(performance.Metadata.NamespaceProperty, performance.Metadata.Name, status.Phase);
        return status.Phase;
    }

    static string ComponentConditionType(string component) => component switch
    {
        "authproxy" => "AuthProxyReady",
        "ante" => "AnteReady",
        _ => "ApplicationReady"
    };

    async Task RollOutAsync(Performance performance, PreparedPerformance prepared, PerformanceStatusBuilder status, CancellationToken cancellationToken)
    {
        var names = PerformanceNames.For(performance);
        var @namespace = performance.Metadata.NamespaceProperty;
        var spec = performance.Spec;

        var stored = SigningKeyState.Deserialize(await cluster.GetSecretValueAsync(@namespace, names.SigningKeysSecret, SigningKeyState.SecretKey, cancellationToken));
        var keys = SigningKeys.Ensure(stored, spec.SigningKeys.Rotation, spec.SigningKeys.RetainPrevious);
        if (stored is null || !string.Equals(stored.Serialize(), keys.Serialize(), StringComparison.Ordinal))
        {
            var secret = SecretFactory.Create(performance, names.SigningKeysSecret, "signing-keys", new Dictionary<string, string> { [SigningKeyState.SecretKey] = keys.Serialize() });
            await cluster.ApplyAsync(secret, cancellationToken);
        }

        performance.Status.SigningKeys = new()
        {
            InvitationKeyId = keys.Invitation.Current.KeyId,
            AttestationKeyId = keys.Attestation.Current.KeyId,
            ObservedRotation = keys.Rotation,
            PinnedPreviousKeys = keys.Invitation.Previous.Count + keys.Attestation.Previous.Count
        };

        PublishedRelease? target = null;
        PublishedInputs? published = null;
        if (prepared.Storage is not null)
        {
            target = PublishedRelease.Of(spec);
            published = new(prepared.Storage, prepared.NotesToken, RolloutTracker.ToDeploy(performance.Status, target));
        }

        var desired = DesiredPerformanceBuilder.Build(performance, prepared.Connection, prepared.Environment, keys, published, prepared.ReadSide);
        foreach (var secret in desired.Secrets)
        {
            await cluster.ApplyAsync(secret, cancellationToken);
        }

        if (desired.NetworkPolicy is not null)
        {
            await cluster.ApplyAsync(desired.NetworkPolicy, cancellationToken);
        }

        foreach (var service in desired.Services)
        {
            await cluster.ApplyAsync(service, cancellationToken);
        }

        foreach (var deployment in desired.Deployments)
        {
            await cluster.ApplyAsync(deployment, cancellationToken);
        }

        if (desired.Ingress is not null)
        {
            await cluster.ApplyAsync(desired.Ingress, cancellationToken);
        }

        var allReady = true;
        var applicationReady = false;
        foreach (var deployment in desired.Deployments)
        {
            var readiness = await cluster.GetDeploymentReadinessAsync(@namespace, deployment.Metadata.Name, cancellationToken);
            var ready = readiness?.IsReady ?? false;
            allReady &= ready;
            applicationReady |= ready && deployment.Metadata.Name == names.Application;
            var component = deployment.Metadata.Labels["app.kubernetes.io/name"];
            status.Set(ComponentConditionType(component), ready, ready ? "Ready" : "NotReady", ready ? $"{deployment.Metadata.Name} is ready." : $"{deployment.Metadata.Name} has {readiness?.Ready ?? 0} of {readiness?.Desired ?? 0} replicas ready.");
        }

        if (published is not null && target is not null)
        {
            RolloutTracker.Observe(performance.Status, published.Release, target, applicationReady, time.GetUtcNow(), TimeSpan.FromSeconds(spec.Application.Published!.RolloutTimeoutSeconds));
            var held = published.Release != target;
            status.Set(
                "RolloutSucceeded",
                !held,
                held ? "RolledBack" : "RolledOut",
                held ? $"Version {target.Version} failed and version {published.Release.Version} keeps running." : $"Version {target.Version} is the version asked for.");
        }

        status.Applied(allReady);
    }

    async Task ExpireAsync(Performance performance, PerformanceStatusBuilder status, CancellationToken cancellationToken)
    {
        var names = PerformanceNames.For(performance);
        var @namespace = performance.Metadata.NamespaceProperty;
        var spec = performance.Spec;

        // The route goes first, so nothing reaches the workloads while they stop.
        if (spec.Ingress.Enabled)
        {
            await cluster.DeleteAsync<V1Ingress>(@namespace, names.Ingress, cancellationToken);
        }

        await cluster.ScaleDeploymentAsync(@namespace, names.Application, 0, cancellationToken);
        if (spec.AuthProxy.Enabled)
        {
            await cluster.ScaleDeploymentAsync(@namespace, names.AuthProxy, 0, cancellationToken);
        }

        if (spec.Onboarding.Enabled)
        {
            await cluster.ScaleDeploymentAsync(@namespace, names.Ante, 0, cancellationToken);
        }

        performance.Status.RunningVersion = string.Empty;
        performance.Status.Rollout = null;
        status.Expire();
    }
}
