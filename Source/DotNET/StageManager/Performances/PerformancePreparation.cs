// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Gathers what a Performance needs before it may roll out, and says plainly what is missing. It changes nothing in the cluster, so a Performance that is not ready is never half applied.
/// </summary>
/// <param name="cluster">The cluster.</param>
/// <param name="resolver">The environment resolver.</param>
public class PerformancePreparation(IClusterResources cluster, IEnvironmentResolver resolver)
{
    /// <summary>
    /// Prepares the Performance.
    /// </summary>
    /// <param name="performance">The Performance.</param>
    /// <param name="status">The status to record conditions in.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>What is needed to roll out, or null when the Performance is blocked, which is then recorded in <paramref name="status"/>.</returns>
    public async Task<PreparedPerformance?> PrepareAsync(Performance performance, PerformanceStatusBuilder status, CancellationToken cancellationToken)
    {
        var problems = SpecValidator.Validate(performance);
        status.Set("SpecValid", problems.Count == 0, problems.Count == 0 ? "Valid" : "InvalidSpec", string.Join(' ', problems));
        if (problems.Count > 0)
        {
            status.Block();
            return null;
        }

        var connection = await ResolveConnectionAsync(performance, status, cancellationToken);
        if (connection is null)
        {
            status.Block();
            return null;
        }

        var environment = await ResolveEnvironmentAsync(performance, status, cancellationToken);
        if (environment is null)
        {
            status.Block();
            return null;
        }

        var missing = environment.Missing(EnvironmentKeys.RequiredBy(performance.Spec));
        status.Set(
            "EnvironmentComplete",
            missing.Count == 0,
            missing.Count == 0 ? "Complete" : "MissingEnvironmentKeys",
            missing.Count == 0 ? "The environment has every key the Performance needs." : $"The environment '{environment.Name}' is missing {missing.Count} key(s): {string.Join(", ", missing)}.");
        if (missing.Count > 0)
        {
            status.Block(missing);
            return null;
        }

        return new(connection, environment);
    }

    async Task<ChronicleConnectionRef?> ResolveConnectionAsync(Performance performance, PerformanceStatusBuilder status, CancellationToken cancellationToken)
    {
        var @namespace = performance.Metadata.NamespaceProperty;
        ChronicleConnectionRef? connection = null;
        var failure = string.Empty;

        if (performance.Spec.Chronicle is { } explicitConnection)
        {
            var reference = explicitConnection.ConnectionSecretRef;
            connection = new(reference.Name, string.IsNullOrEmpty(reference.Key) ? ChronicleConnectionRef.DefaultKey : reference.Key);
        }
        else if (performance.Spec.ProductionRef is { } production)
        {
            var secret = await cluster.GetProductionConnectionSecretNameAsync(production.Namespace ?? @namespace, production.Name, cancellationToken);
            if (secret is null)
            {
                failure = $"The Production '{production.Name}' does not exist or reports no connection Secret yet.";
            }
            else
            {
                connection = new(secret, ChronicleConnectionRef.DefaultKey);
            }
        }

        if (connection is not null && await cluster.GetSecretValueAsync(@namespace, connection.SecretName, connection.Key, cancellationToken) is null)
        {
            failure = $"The connection Secret '{connection.SecretName}' has no key '{connection.Key}'.";
            connection = null;
        }

        status.Set("ChronicleConnection", connection is not null, connection is not null ? "Resolved" : "ChronicleNotAvailable", connection is not null ? $"Using the connection Secret '{connection.SecretName}'." : failure);
        return connection;
    }

    async Task<ResolvedEnvironment?> ResolveEnvironmentAsync(Performance performance, PerformanceStatusBuilder status, CancellationToken cancellationToken)
    {
        var @namespace = performance.Metadata.NamespaceProperty;
        var spec = performance.Spec.Environment;
        var key = string.IsNullOrEmpty(spec.ConfigMapRef.Key) ? $"{spec.Name}.yaml" : spec.ConfigMapRef.Key;
        var file = await cluster.GetConfigMapValueAsync(@namespace, spec.ConfigMapRef.Name, key, cancellationToken);
        if (file is null)
        {
            status.Set("EnvironmentResolved", false, "EnvironmentFileNotFound", $"The ConfigMap '{spec.ConfigMapRef.Name}' has no key '{key}'.");
            return null;
        }

        string? passphrase = null;
        if (spec.PassphraseSecretRef is { } reference)
        {
            passphrase = await cluster.GetSecretValueAsync(@namespace, reference.Name, string.IsNullOrEmpty(reference.Key) ? "passphrase" : reference.Key, cancellationToken);
            if (passphrase is null)
            {
                status.Set("EnvironmentResolved", false, "PassphraseNotFound", $"The passphrase Secret '{reference.Name}' does not exist or has no such key.");
                return null;
            }
        }

        var resolution = await resolver.ResolveAsync(spec.Name, file, passphrase, cancellationToken);
        status.Set("EnvironmentResolved", resolution.Environment is not null, resolution.Environment is not null ? "Resolved" : "EnvironmentNotResolved", resolution.Environment is not null ? $"Resolved the environment '{spec.Name}'." : resolution.Failure);
        return resolution.Environment;
    }
}
