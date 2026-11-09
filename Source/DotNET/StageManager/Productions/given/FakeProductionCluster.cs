// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Showrunner.StageManager.Performances;

namespace Cratis.Showrunner.StageManager.Productions.given;

/// <summary>
/// A cluster that records what was applied and serves configured Secrets, definitions and readiness.
/// </summary>
public class FakeProductionCluster : IProductionCluster
{
    public Dictionary<string, string> SecretValues { get; } = new(StringComparer.Ordinal);

    public HashSet<string> Definitions { get; } = new(StringComparer.Ordinal)
    {
        CertManagerCertificate.DefinitionName,
        CertManagerIssuer.DefinitionName,
        PerconaServerMongoDB.DefinitionName
    };

    public string? MongoDBState { get; set; }

    public bool StatefulSetsReady { get; set; }

    public List<IKubernetesObject<V1ObjectMeta>> Applied { get; } = [];

    public Production? StatusWritten { get; private set; }

    public IEnumerable<T> AppliedOf<T>() => Applied.OfType<T>();

    public T? AppliedNamed<T>(string name)
        where T : IKubernetesObject<V1ObjectMeta> => Applied.OfType<T>().SingleOrDefault(resource => resource.Metadata.Name == name);

    public string? AppliedSecretValue(string name, string key) =>
        AppliedNamed<V1Secret>(name)?.Data is { } data && data.TryGetValue(key, out var bytes) ? Encoding.UTF8.GetString(bytes) : null;

    public Task<string?> GetSecretValueAsync(string @namespace, string name, string key, CancellationToken cancellationToken) =>
        Task.FromResult(AppliedSecretValue(name, key) ?? SecretValues.GetValueOrDefault($"{@namespace}/{name}/{key}"));

    public Task<bool> HasDefinitionAsync(string name, CancellationToken cancellationToken) => Task.FromResult(Definitions.Contains(name));

    public Task ApplyAsync<TResource>(TResource desired, CancellationToken cancellationToken)
        where TResource : IKubernetesObject<V1ObjectMeta>
    {
        Applied.RemoveAll(existing => existing.GetType() == typeof(TResource) && existing.Metadata.Name == desired.Metadata.Name);
        Applied.Add(desired);
        return Task.CompletedTask;
    }

    public Task<string?> GetMongoDBStateAsync(string @namespace, string name, CancellationToken cancellationToken) => Task.FromResult(MongoDBState);

    public Task<DeploymentReadiness?> GetStatefulSetReadinessAsync(string @namespace, string name, CancellationToken cancellationToken)
    {
        var statefulSet = AppliedNamed<V1StatefulSet>(name);
        if (statefulSet is null)
        {
            return Task.FromResult<DeploymentReadiness?>(null);
        }

        var replicas = statefulSet.Spec.Replicas ?? 1;
        return Task.FromResult<DeploymentReadiness?>(StatefulSetsReady ? new(replicas, replicas, replicas, replicas) : new(replicas, 0, 0, replicas));
    }

    public Task UpdateStatusAsync(Production production, CancellationToken cancellationToken)
    {
        StatusWritten = production;
        return Task.CompletedTask;
    }
}
#endif
