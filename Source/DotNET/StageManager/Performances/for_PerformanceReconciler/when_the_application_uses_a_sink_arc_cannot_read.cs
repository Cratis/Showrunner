// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_application_uses_a_sink_arc_cannot_read : given_a_performance_on_a_production
{
    void Establish() => _performance.Spec.Application.Sinks = ["reporting", "organization"];

    Task Because() => Reconcile();

    [Fact] void should_still_roll_out() => _phase.ShouldEqual(PerformancePhase.Progressing);
    [Fact] void should_warn_that_a_sink_is_not_readable() => Condition("SinksReadable").Status.ShouldEqual("False");
    [Fact] void should_give_the_reason() => Condition("SinksReadable").Reason.ShouldEqual("NotReadableByArc");
    [Fact] void should_name_the_sql_sink() => Condition("SinksReadable").Message.Contains("reporting", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_name_the_neo4j_sink_on_another_server() => Condition("SinksReadable").Message.Contains("organization", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_bind_no_sink() => Application.Env.Any(variable => variable.Name.StartsWith("Cratis__MongoDB__Sinks__", StringComparison.Ordinal)).ShouldBeFalse();
}
#endif
