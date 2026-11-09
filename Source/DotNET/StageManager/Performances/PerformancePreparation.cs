// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Productions;

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Gathers what a Performance needs before it may roll out, and says plainly what is missing. It changes nothing in the cluster, so a Performance that is not ready is never half applied.
/// </summary>
/// <param name="cluster">The cluster.</param>
/// <param name="resolver">The environment resolver.</param>
public class PerformancePreparation(IClusterResources cluster, IEnvironmentResolver resolver)
{
    readonly ApplicationReadSideBinding _readSides = new(cluster);

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

        ChronicleConnectionRef? connection = null;
        PublishedSecrets? published = null;
        ApplicationReadSide? readSide = null;
        if (PublishedStageContract.IsPublished(performance.Spec))
        {
            published = await ResolvePublishedAsync(performance, status, cancellationToken);
            if (published is null)
            {
                status.Block();
                return null;
            }
        }
        else
        {
            connection = await ResolveConnectionAsync(performance, status, cancellationToken);
            var sinks = connection is null ? SinkCheck.Failed : await CheckSinksAsync(performance, status, cancellationToken);
            readSide = sinks.Production is { } production ? await _readSides.BindAsync(performance, production, status, cancellationToken) : ApplicationReadSide.None;
            if (connection is null || !sinks.Mapped || readSide is null)
            {
                status.Block();
                return null;
            }
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

        return new(connection, environment, published?.Storage, published?.NotesToken, readSide);
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

    async Task<SinkCheck> CheckSinksAsync(Performance performance, PerformanceStatusBuilder status, CancellationToken cancellationToken)
    {
        if (performance.Spec.ProductionRef is not { } reference)
        {
            return SinkCheck.WithoutProduction;
        }

        var used = ApplicationSinks.UsedBy(performance.Spec.Application);
        var production = await cluster.GetProductionSpecAsync(reference.Namespace ?? performance.Metadata.NamespaceProperty, reference.Name, cancellationToken);
        if (production is null)
        {
            status.Set("SinksMapped", false, "ProductionNotFound", $"The Production '{reference.Name}' does not exist, so its sinks cannot be checked.");
            return SinkCheck.Failed;
        }

        var missing = ApplicationSinks.MissingFrom(used, production);
        var (reason, message) = (used.Count, missing.Count) switch
        {
            (0, _) => ("NoSinksUsed", "The application uses no named sinks."),
            (_, 0) => ("Mapped", $"The Production '{reference.Name}' configures every sink the application uses: {string.Join(", ", used)}."),
            _ => ("MissingSinks", $"The Production '{reference.Name}' does not configure {missing.Count} sink(s) the application uses: {string.Join(", ", missing)}.")
        };
        status.Set("SinksMapped", missing.Count == 0, reason, message);
        return missing.Count == 0 ? new(true, production) : SinkCheck.Failed;
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

    async Task<PublishedSecrets?> ResolvePublishedAsync(Performance performance, PerformanceStatusBuilder status, CancellationToken cancellationToken)
    {
        var @namespace = performance.Metadata.NamespaceProperty;
        var published = performance.Spec.Application.Published!;

        var key = PublishedStageContract.PackageKey(published);
        var package = await cluster.GetPackageStateAsync(@namespace, published.PackageConfigMapRef.Name, key, cancellationToken);
        var (packageReason, packageMessage) = package switch
        {
            null => ("PackageNotFound", $"The package ConfigMap '{published.PackageConfigMapRef.Name}' does not exist."),
            { HasKey: false } => ("PackageKeyNotFound", $"The package ConfigMap '{published.PackageConfigMapRef.Name}' has no key '{key}'."),
            { Immutable: false } => ("PackageNotImmutable", $"The package ConfigMap '{published.PackageConfigMapRef.Name}' is not immutable: a version must never change in place."),
            _ => ("PackageAvailable", $"Using the package in the ConfigMap '{published.PackageConfigMapRef.Name}'.")
        };
        var packageFound = packageReason == "PackageAvailable";
        status.Set("PackageAvailable", packageFound, packageReason, packageMessage);

        var storage = await ResolveSecretAsync(performance, status, "StorageConnection", published.StorageConnectionSecretRef, ChronicleConnectionRef.DefaultKey, "storage connection Secret", cancellationToken);

        ResolvedSecretRef? notesToken = null;
        var notesFound = true;
        if (published.Notes is { Enabled: true } notes)
        {
            notesToken = await ResolveSecretAsync(performance, status, "NotesCollectorToken", notes.CollectorTokenSecretRef, PublishedStageContract.NotesTokenFile, "Notes collector token Secret", cancellationToken);
            notesFound = notesToken is not null;
        }

        return packageFound && storage is not null && notesFound ? new(storage, notesToken) : null;
    }

    async Task<ResolvedSecretRef?> ResolveSecretAsync(Performance performance, PerformanceStatusBuilder status, string condition, SecretKeyReference reference, string defaultKey, string description, CancellationToken cancellationToken)
    {
        var key = string.IsNullOrEmpty(reference.Key) ? defaultKey : reference.Key;
        var found = await cluster.GetSecretValueAsync(performance.Metadata.NamespaceProperty, reference.Name, key, cancellationToken) is not null;
        status.Set(condition, found, found ? "Resolved" : "SecretNotFound", found ? $"Using the {description} '{reference.Name}'." : $"The {description} '{reference.Name}' does not exist or has no key '{key}'.");
        return found ? new(reference.Name, key) : null;
    }

    sealed record SinkCheck(bool Mapped, ProductionSpec? Production)
    {
        public static SinkCheck Failed { get; } = new(false, null);

        public static SinkCheck WithoutProduction { get; } = new(true, null);
    }

    sealed record PublishedSecrets(ResolvedSecretRef Storage, ResolvedSecretRef? NotesToken);
}
