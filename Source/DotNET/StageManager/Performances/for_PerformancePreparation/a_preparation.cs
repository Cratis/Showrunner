// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;
using Cratis.Showrunner.StageManager.Resources;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

/// <summary>
/// The base of the preparation specifications: a Performance, a fake cluster and a resolver that records what it was asked to resolve.
/// </summary>
public abstract class a_preparation : Specification
{
    protected Performance _performance;
    protected FakeCluster _cluster;
    protected ResolvedEnvironment _environment;
    protected RecordingResolver _resolver;
    protected PerformanceStatusBuilder _status;
    protected PreparedPerformance? _prepared;

    protected virtual Performance CreatePerformance() => PerformanceSamples.Performance();

    protected virtual EnvironmentResolution CreateResolution() => EnvironmentResolution.Resolved(PerformanceSamples.Environment());

    protected string Conditions => string.Join(" | ", _performance.Status.Conditions.Select(condition => $"{condition.Type}={condition.Status}/{condition.Reason}"));

    protected ResourceCondition Condition(string type) => _performance.Status.Conditions.Single(condition => condition.Type == type);

    void Establish()
    {
        _performance = CreatePerformance();
        _performance.Status = new();
        _cluster = new FakeCluster();
        _resolver = new RecordingResolver(CreateResolution());
    }

    /// <summary>
    /// Prepares the Performance and writes the collected conditions to its status. Each specification calls it from its own Because().
    /// </summary>
    /// <returns>A task that completes when the Performance is prepared.</returns>
    protected async Task Prepare()
    {
        _status = new PerformanceStatusBuilder(_performance, TimeProvider.System);
        _prepared = await new PerformancePreparation(_cluster, _resolver).PrepareAsync(_performance, _status, CancellationToken.None);
        _status.Write();
    }

    /// <summary>
    /// A resolver that returns a fixed resolution and remembers what it was given.
    /// </summary>
    /// <param name="resolution">The resolution to return.</param>
    protected sealed class RecordingResolver(EnvironmentResolution resolution) : IEnvironmentResolver
    {
        public bool Called { get; private set; }

        public string? File { get; private set; }

        public string? Passphrase { get; private set; }

        public Task<EnvironmentResolution> ResolveAsync(string name, string environmentFile, string? passphrase, CancellationToken cancellationToken)
        {
            Called = true;
            File = environmentFile;
            Passphrase = passphrase;
            return Task.FromResult(resolution);
        }
    }
}
#endif
