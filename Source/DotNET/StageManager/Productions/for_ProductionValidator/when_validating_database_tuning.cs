// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionValidator;

public class when_validating_database_tuning : Specification
{
    IReadOnlyList<ProductionProblem> _tunedManaged;
    IReadOnlyList<ProductionProblem> _ratioOfOne;
    IReadOnlyList<ProductionProblem> _negativeRatio;
    IReadOnlyList<ProductionProblem> _negativeRate;
    IReadOnlyList<ProductionProblem> _metricsOnExternal;
    IReadOnlyList<ProductionProblem> _tuningOnExternal;

    static IReadOnlyList<ProductionProblem> On(Action<ProductionDatabase> change, bool managed)
    {
        var spec = managed ? ProductionSamples.Managed() : ProductionSamples.External();
        change(spec.Database);
        return ProductionValidator.ValidateEach(spec);
    }

    void Because()
    {
        _tunedManaged = On(
            database =>
            {
                database.WiredTigerCacheSizeRatio = 0.39;
                database.TcmallocReleaseRate = 0;
                database.Metrics.Enabled = true;
            },
            managed: true);
        _ratioOfOne = On(database => database.WiredTigerCacheSizeRatio = 1, managed: true);
        _negativeRatio = On(database => database.WiredTigerCacheSizeRatio = -0.1, managed: true);
        _negativeRate = On(database => database.TcmallocReleaseRate = -1, managed: true);
        _metricsOnExternal = On(database => database.Metrics.Enabled = true, managed: false);
        _tuningOnExternal = On(database => database.WiredTigerCacheSizeRatio = 0.4, managed: false);
    }

    [Fact] void should_accept_a_tuned_managed_database() => _tunedManaged.ShouldBeEmpty();
    [Fact] void should_refuse_a_ratio_of_one() => _ratioOfOne.Single().Message.ShouldContain("wiredTigerCacheSizeRatio must be above 0 and below 1");
    [Fact] void should_refuse_a_negative_ratio() => _negativeRatio.Single().Message.ShouldContain("wiredTigerCacheSizeRatio must be above 0 and below 1");
    [Fact] void should_refuse_a_negative_release_rate() => _negativeRate.Single().Message.ShouldContain("tcmallocReleaseRate cannot be negative");
    [Fact] void should_refuse_metrics_on_an_external_database() => _metricsOnExternal.Single().Message.ShouldContain("database.metrics needs database.mode Managed");
    [Fact] void should_refuse_tuning_an_external_database() => _tuningOnExternal.Single().Message.ShouldContain("need database.mode Managed");
}
#endif
