// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Builds the desired state of a Performance from its spec, the resolved environment, the Chronicle connection and the signing keys. It is a pure function: nothing is read from or written to the cluster here.
/// </summary>
public static class DesiredPerformanceBuilder
{
    const string ApplicationSecretsDirectory = "/run/secrets/cratis";

    /// <summary>
    /// Builds the desired state.
    /// </summary>
    /// <param name="performance">The Performance.</param>
    /// <param name="connection">The Chronicle connection Secret to mount.</param>
    /// <param name="environment">The resolved Cratis environment.</param>
    /// <param name="keys">The generated signing keys.</param>
    /// <returns>The desired state.</returns>
    public static DesiredPerformance Build(Performance performance, ChronicleConnectionRef connection, ResolvedEnvironment environment, SigningKeyState keys)
    {
        var spec = performance.Spec;
        var names = PerformanceNames.For(performance);
        var revisionKey = keys.Invitation.Current.PrivateKeyPem;
        var secrets = new List<V1Secret>();
        var workloads = new List<WorkloadDefinition>();

        var applicationFiles = environment.Values
            .Where(pair => EnvironmentKeys.IsForApplication(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        secrets.Add(SecretFactory.Create(performance, names.ApplicationSecret, "application", applicationFiles));
        workloads.Add(Application(performance, names, connection, applicationFiles, revisionKey));

        if (spec.AuthProxy.Enabled)
        {
            var files = AuthProxyContract.SecretFiles(performance, environment, keys);
            secrets.Add(SecretFactory.Create(performance, names.AuthProxySecret, "authproxy", files));
            workloads.Add(AuthProxy(performance, names, files, revisionKey));
        }

        if (spec.Onboarding.Enabled)
        {
            var files = AnteContract.SecretFiles(performance, environment, keys);
            secrets.Add(SecretFactory.Create(performance, names.AnteSecret, "ante", files));
            workloads.Add(Ante(performance, names, connection, files, revisionKey));
        }

        return new(
            secrets,
            [.. workloads.Select(workload => WorkloadFactory.Deployment(performance, workload))],
            [.. workloads.Select(workload => WorkloadFactory.Service(performance, workload))],
            spec.Ingress.Enabled ? IngressFactory.Create(performance) : null);
    }

    static SecretSource ConnectionSource(ChronicleConnectionRef connection) =>
        new(connection.SecretName, new Dictionary<string, string> { [connection.Key] = ChronicleConnectionRef.FileName });

    static WorkloadDefinition Application(Performance performance, PerformanceNames names, ChronicleConnectionRef connection, IReadOnlyDictionary<string, string> files, string revisionKey)
    {
        var spec = performance.Spec;
        Dictionary<string, string> variables = new(StringComparer.Ordinal)
        {
            ["CRATIS_ENVIRONMENT"] = spec.Environment.Name,
            ["CRATIS_SECRETS_DIRECTORY"] = ApplicationSecretsDirectory,
            ["CRATIS_CHRONICLE_CONNECTION_STRING_FILE"] = $"{ApplicationSecretsDirectory}/{ChronicleConnectionRef.FileName}"
        };

        return new(
            names.Application,
            "application",
            spec.Application.Image,
            spec.Application.Port,
            spec.Application.Replicas,
            variables,
            [new("environment", ApplicationSecretsDirectory, [new(names.ApplicationSecret), ConnectionSource(connection)])],
            [],
            ConfigRevision.Of(revisionKey, files));
    }

    static WorkloadDefinition AuthProxy(Performance performance, PerformanceNames names, IReadOnlyDictionary<string, string> files, string revisionKey)
    {
        var proxy = performance.Spec.AuthProxy;
        return new(
            names.AuthProxy,
            "authproxy",
            proxy.Image,
            proxy.Port,
            proxy.Replicas,
            AuthProxyContract.EnvironmentVariables(performance),
            [new("environment", AuthProxyContract.SecretsDirectory, [new(names.AuthProxySecret)])],
            [],
            ConfigRevision.Of(revisionKey, files));
    }

    static WorkloadDefinition Ante(Performance performance, PerformanceNames names, ChronicleConnectionRef connection, IReadOnlyDictionary<string, string> files, string revisionKey)
    {
        var onboarding = performance.Spec.Onboarding;
        List<ConfigMapMount> configMaps = [];
        if (onboarding.Legal is { } legal)
        {
            configMaps.Add(new("legal", AnteContract.LegalDirectory, legal.ConfigMapRef.Name));
        }

        // Exactly one Ante per Performance, so one replica: it owns its own event store and its own state.
        return new(
            names.Ante,
            "ante",
            onboarding.Image,
            onboarding.Port,
            1,
            AnteContract.EnvironmentVariables(performance),
            [new("secrets", AnteContract.SecretsDirectory, [new(names.AnteSecret), ConnectionSource(connection)])],
            configMaps,
            ConfigRevision.Of(revisionKey, files));
    }
}
