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
        var prepared = await preparation.PrepareAsync(performance, status, cancellationToken);
        if (prepared is not null)
        {
            await RollOutAsync(performance, prepared, status, cancellationToken);
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

        var desired = DesiredPerformanceBuilder.Build(performance, prepared.Connection, prepared.Environment, keys);
        foreach (var secret in desired.Secrets)
        {
            await cluster.ApplyAsync(secret, cancellationToken);
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
        foreach (var deployment in desired.Deployments)
        {
            var readiness = await cluster.GetDeploymentReadinessAsync(@namespace, deployment.Metadata.Name, cancellationToken);
            var ready = readiness?.IsReady ?? false;
            allReady &= ready;
            var component = deployment.Metadata.Labels["app.kubernetes.io/name"];
            status.Set(ComponentConditionType(component), ready, ready ? "Ready" : "NotReady", ready ? $"{deployment.Metadata.Name} is ready." : $"{deployment.Metadata.Name} has {readiness?.Ready ?? 0} of {readiness?.Desired ?? 0} replicas ready.");
        }

        status.Applied(allReady);
    }
}
