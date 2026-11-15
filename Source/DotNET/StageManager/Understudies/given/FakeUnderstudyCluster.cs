// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Showrunner.StageManager.Productions;

namespace Cratis.Showrunner.StageManager.Understudies.given;

/// <summary>
/// A cluster that records what was applied and serves configured Productions, Secrets and Jobs.
/// </summary>
public class FakeUnderstudyCluster : IUnderstudyCluster
{
    public Production? Production { get; set; }

    public Dictionary<string, string> SecretValues { get; } = new(StringComparer.Ordinal);

    public List<IKubernetesObject<V1ObjectMeta>> Applied { get; } = [];

    public List<IKubernetesObject<V1ObjectMeta>> Created { get; } = [];

    public List<V1Job> Jobs { get; } = [];

    public Understudy? StatusWritten { get; private set; }

    public IEnumerable<T> AppliedOf<T>() => Applied.OfType<T>();

    public T? AppliedNamed<T>(string name)
        where T : IKubernetesObject<V1ObjectMeta> => Applied.OfType<T>().SingleOrDefault(resource => resource.Metadata.Name == name);

    public Task<Production?> GetProductionAsync(string @namespace, string name, CancellationToken cancellationToken) =>
        Task.FromResult(Production is not null && Production.Metadata.Name == name ? Production : null);

    public Task<string?> GetSecretValueAsync(string @namespace, string name, string key, CancellationToken cancellationToken) =>
        Task.FromResult(SecretValues.GetValueOrDefault($"{@namespace}/{name}/{key}"));

    public Task ApplyAsync<TResource>(TResource desired, CancellationToken cancellationToken)
        where TResource : IKubernetesObject<V1ObjectMeta>
    {
        Applied.RemoveAll(existing => existing.GetType() == typeof(TResource) && existing.Metadata.Name == desired.Metadata.Name);
        Applied.Add(desired);
        return Task.CompletedTask;
    }

    public Task<bool> TryCreateAsync<TResource>(TResource desired, CancellationToken cancellationToken)
        where TResource : IKubernetesObject<V1ObjectMeta>
    {
        if (Created.Exists(existing => existing.GetType() == typeof(TResource) && existing.Metadata.Name == desired.Metadata.Name))
        {
            return Task.FromResult(false);
        }

        Created.Add(desired);
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<V1Job>> ListJobsAsync(string @namespace, string understudy, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<V1Job>>([.. Jobs.Concat(Created.OfType<V1Job>())]);

    public Task UpdateStatusAsync(Understudy understudy, CancellationToken cancellationToken)
    {
        StatusWritten = understudy;
        return Task.CompletedTask;
    }

    public void AddSecret(string name, string key, string value) => SecretValues[$"apps/{name}/{key}"] = value;

    public static string Utf8(byte[] bytes) => Encoding.UTF8.GetString(bytes);
}
#endif
