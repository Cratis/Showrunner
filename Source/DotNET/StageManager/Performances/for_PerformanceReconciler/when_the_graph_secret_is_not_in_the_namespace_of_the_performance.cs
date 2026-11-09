// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_graph_secret_is_not_in_the_namespace_of_the_performance : given_a_performance_on_a_production
{
    void Establish()
    {
        _performance.Spec.Application.Sinks = ["people"];
        _cluster.SecretValues.Remove("apps/ledger-neo4j/password");
    }

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.Count.ShouldEqual(0);
    [Fact] void should_not_have_its_graph_bound() => Condition("GraphBound").Status.ShouldEqual("False");
    [Fact] void should_give_the_reason() => Condition("GraphBound").Reason.ShouldEqual("SecretNotInNamespace");
    [Fact] void should_name_the_secret() => Condition("GraphBound").Message.Contains("'ledger-neo4j'", StringComparison.Ordinal).ShouldBeTrue();
}
#endif
