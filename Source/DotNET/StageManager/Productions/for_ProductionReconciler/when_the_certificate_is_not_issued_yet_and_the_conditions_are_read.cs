// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_certificate_is_not_issued_yet_and_the_conditions_are_read : a_production_reconciliation
{
    void Establish()
    {
        _cluster.SecretValues.Remove("apps/ledger-chronicle-tls/keystore.p12");
        _cluster.SecretValues.Remove("apps/ledger-chronicle-tls/ca.crt");
    }

    Task Because() => Reconcile();

    [Fact] void should_say_the_connection_waits_for_the_certificates() => Condition(ProductionConditions.ConnectionAvailable).Reason.ShouldEqual("WaitingForCertificates");
    [Fact] void should_say_the_connection_is_not_available() => Condition(ProductionConditions.ConnectionAvailable).Status.ShouldEqual("False");
    [Fact] void should_say_chronicle_waits_for_its_dependencies() => Condition(ProductionConditions.ChronicleReady).Reason.ShouldEqual("WaitingForDependencies");
    [Fact] void should_still_apply_the_database_condition() => Condition(ProductionConditions.DatabaseReady).Reason.ShouldEqual("External");
    [Fact] void should_still_apply_the_graph_condition() => Condition(ProductionConditions.GraphReady).Reason.ShouldEqual("NoGraph");
    [Fact] void should_not_run_chronicle_service() => _cluster.AppliedOf<V1Service>().Any(service => service.Metadata.Name == "ledger-chronicle").ShouldBeFalse();
    [Fact] void should_still_set_the_chronicle_address() => _production.Status.ChronicleAddress.ShouldNotEqual(string.Empty);
    [Fact] void should_record_the_conditions_in_order() => string.Join(',', _production.Status.Conditions.Select(condition => condition.Type).Where(type => new[] { ProductionConditions.CertificatesReady, ProductionConditions.DatabaseReady, ProductionConditions.GraphReady, ProductionConditions.ConnectionAvailable, ProductionConditions.ChronicleReady }.Contains(type, StringComparer.Ordinal))).ShouldEqual("CertificatesReady,DatabaseReady,GraphReady,ConnectionAvailable,ChronicleReady");
}
#endif
