// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Showrunner.StageManager.Productions;

namespace Cratis.Showrunner.StageManager.Performances.given;

/// <summary>
/// A cluster that records what was applied and serves configured ConfigMaps and Secrets.
/// </summary>
public class FakeCluster : IClusterResources
{
    public Dictionary<string, string> ConfigMapValues { get; } = new(StringComparer.Ordinal)
    {
        ["apps/crm-environment/production.yaml"] = "environment-file"
    };

    public Dictionary<string, string> SecretValues { get; } = new(StringComparer.Ordinal)
    {
        ["apps/chronicle-connection/connectionString"] = PerformanceSamples.ConnectionString,
        ["apps/crm-storage/connectionString"] = PerformanceSamples.StorageConnectionString,
        ["apps/crm-notes-token/token"] = PerformanceSamples.NotesToken
    };

    public Dictionary<string, PackageState> Packages { get; } = new(StringComparer.Ordinal)
    {
        ["apps/crm-package-1-4-0"] = new(true, true),
        ["apps/crm-package-1-5-0"] = new(true, true)
    };

    public List<string> Deleted { get; } = [];

    public Dictionary<string, int> Scaled { get; } = new(StringComparer.Ordinal);

    public UsageSample? Usage { get; set; }

    public Dictionary<string, string> ProductionConnectionSecrets { get; } = new(StringComparer.Ordinal);

    public Dictionary<string, ProductionSpec> Productions { get; } = new(StringComparer.Ordinal);

    public List<IKubernetesObject<V1ObjectMeta>> Applied { get; } = [];

    public Performance? StatusWritten { get; private set; }

    public bool DeploymentsReady { get; set; }

    public IEnumerable<T> AppliedOf<T>() => Applied.OfType<T>();

    public Task<string?> GetConfigMapValueAsync(string @namespace, string name, string key, CancellationToken cancellationToken) =>
        Task.FromResult(ConfigMapValues.GetValueOrDefault($"{@namespace}/{name}/{key}"));

    public Task<string?> GetSecretValueAsync(string @namespace, string name, string key, CancellationToken cancellationToken)
    {
        // Secrets the operator applied itself are readable afterwards, as they are in a cluster.
        var applied = Applied.OfType<V1Secret>().FirstOrDefault(secret => secret.Metadata.Name == name);
        if (applied?.Data is { } data && data.TryGetValue(key, out var bytes))
        {
            return Task.FromResult<string?>(Encoding.UTF8.GetString(bytes));
        }

        return Task.FromResult(SecretValues.GetValueOrDefault($"{@namespace}/{name}/{key}"));
    }

    public Task<string?> GetProductionConnectionSecretNameAsync(string @namespace, string name, CancellationToken cancellationToken) =>
        Task.FromResult(ProductionConnectionSecrets.GetValueOrDefault($"{@namespace}/{name}"));

    public Task<ProductionSpec?> GetProductionSpecAsync(string @namespace, string name, CancellationToken cancellationToken) =>
        Task.FromResult(Productions.GetValueOrDefault($"{@namespace}/{name}"));

    public Task ApplyAsync<TResource>(TResource desired, CancellationToken cancellationToken)
        where TResource : IKubernetesObject<V1ObjectMeta>
    {
        Applied.RemoveAll(existing => existing.GetType() == typeof(TResource) && existing.Metadata.Name == desired.Metadata.Name);
        Applied.Add(desired);
        return Task.CompletedTask;
    }

    public Task<DeploymentReadiness?> GetDeploymentReadinessAsync(string @namespace, string name, CancellationToken cancellationToken) =>
        Task.FromResult<DeploymentReadiness?>(DeploymentsReady ? new(1, 1, 1) : new(1, 0, 0));

    public Task<PackageState?> GetPackageStateAsync(string @namespace, string name, string key, CancellationToken cancellationToken) =>
        Task.FromResult(Packages.TryGetValue($"{@namespace}/{name}", out var state) ? state : null);

    public Task DeleteAsync<TResource>(string @namespace, string name, CancellationToken cancellationToken)
        where TResource : IKubernetesObject<V1ObjectMeta>
    {
        Deleted.Add($"{typeof(TResource).Name}/{name}");
        return Task.CompletedTask;
    }

    public Task ScaleDeploymentAsync(string @namespace, string name, int replicas, CancellationToken cancellationToken)
    {
        Scaled[name] = replicas;
        return Task.CompletedTask;
    }

    public Task<UsageSample?> GetUsageAsync(string @namespace, string performance, CancellationToken cancellationToken) =>
        Task.FromResult(Usage);

    public Task UpdateStatusAsync(Performance performance, CancellationToken cancellationToken)
    {
        StatusWritten = performance;
        return Task.CompletedTask;
    }
}
#endif
