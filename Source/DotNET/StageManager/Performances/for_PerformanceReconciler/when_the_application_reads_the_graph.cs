// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_application_reads_the_graph : given_a_performance_on_a_production
{
    void Establish() => _performance.Spec.Application.Graph = true;

    Task Because() => Reconcile();

    V1EnvVar Variable(string name) => Application.Env.Single(variable => variable.Name == name);

    [Fact] void should_roll_out() => _phase.ShouldEqual(PerformancePhase.Progressing);
    [Fact] void should_have_its_graph_bound() => Condition("GraphBound").Status.ShouldEqual("True");
    [Fact] void should_say_the_credential_is_shared_on_community() => Condition("GraphBound").Reason.ShouldEqual("SharedCredential");
    [Fact] void should_give_the_bolt_address_as_a_value() => Variable("Cratis__Neo4j__Server").Value.ShouldEqual("bolt://ledger-neo4j.apps.svc.cluster.local:7687");
    [Fact] void should_read_the_single_database() => Variable("Cratis__Neo4j__DatabaseMode").Value.ShouldEqual("Single");
    [Fact] void should_read_as_the_graph_user() => Variable("Cratis__Neo4j__UserName").Value.ShouldEqual("neo4j");
    [Fact] void should_name_the_event_store() => Variable("Cratis__Neo4j__EventStore").Value.ShouldEqual("Crm");
    [Fact] void should_take_the_password_from_the_graph_secret() => Variable("Cratis__Neo4j__Password").ValueFrom.SecretKeyRef.Name.ShouldEqual("ledger-neo4j");
    [Fact] void should_take_the_password_key() => Variable("Cratis__Neo4j__Password").ValueFrom.SecretKeyRef.Key.ShouldEqual("password");
    [Fact] void should_not_put_the_password_in_the_deployment() => JsonSerializer.Serialize(ApplicationDeployment).Contains(PerformanceSamples.GraphPassword, StringComparison.Ordinal).ShouldBeFalse();
}
#endif
