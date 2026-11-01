// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.given;

/// <summary>
/// The base of the reconciliation specifications: a Performance, a fake cluster and an environment resolver that resolves what the spec gives it.
/// </summary>
public abstract class a_reconciliation : Specification
{
    protected Performance _performance;
    protected FakeCluster _cluster;
    protected PerformancePhase _phase;
    protected ResolvedEnvironment _environment;

    protected virtual Performance CreatePerformance() => PerformanceSamples.Performance();

    protected virtual ResolvedEnvironment CreateEnvironment() => PerformanceSamples.Environment();

    protected string StatusAsJson => JsonSerializer.Serialize(_performance.Status);

    void Establish()
    {
        _performance = CreatePerformance();
        _environment = CreateEnvironment();
        _cluster = new FakeCluster();
    }

    async Task Because()
    {
        var resolver = new FakeEnvironmentResolver(EnvironmentResolution.Resolved(_environment));
        var reconciler = new PerformanceReconciler(
            _cluster,
            new PerformancePreparation(_cluster, resolver),
            TimeProvider.System,
            Substitute.For<ILogger<PerformanceReconciler>>());
        _phase = await reconciler.ReconcileAsync(_performance, CancellationToken.None);
    }
}
#endif
