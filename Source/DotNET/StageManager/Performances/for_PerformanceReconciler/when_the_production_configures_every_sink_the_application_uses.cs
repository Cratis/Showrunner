// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_production_configures_every_sink_the_application_uses : given_a_performance_on_a_production
{
    void Establish() => _performance.Spec.Application.Sinks = ["Reporting", "people"];

    Task Because() => Reconcile();

    [Fact] void should_roll_out() => _phase.ShouldEqual(PerformancePhase.Progressing);
    [Fact] void should_have_its_sinks_mapped() => SinksMapped.Status.ShouldEqual("True");
    [Fact] void should_give_the_reason() => SinksMapped.Reason.ShouldEqual("Mapped");
}
#endif
