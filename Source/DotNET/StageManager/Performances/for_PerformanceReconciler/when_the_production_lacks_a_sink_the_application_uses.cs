// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_production_lacks_a_sink_the_application_uses : given_a_performance_on_a_production
{
    void Establish() => _performance.Spec.Application.Sinks = ["reporting", "audit"];

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.Count.ShouldEqual(0);
    [Fact] void should_not_have_its_sinks_mapped() => SinksMapped.Status.ShouldEqual("False");
    [Fact] void should_give_the_reason() => SinksMapped.Reason.ShouldEqual("MissingSinks");
    [Fact] void should_name_the_missing_sink() => SinksMapped.Message.Contains("audit", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_not_name_a_configured_sink_as_missing() => SinksMapped.Message.Contains("reporting", StringComparison.Ordinal).ShouldBeFalse();
}
#endif
