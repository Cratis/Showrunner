// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_a_first_reconciliation_applies_every_resource : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.Managed();
        spec.Graph = new() { Mode = ProductionModes.Managed };
        return spec;
    }

    void Establish()
    {
        _cluster.MongoDBState = ManagedMongoDB.ReadyState;
        _cluster.SecretValues[$"apps/ledger-mongodb-users/{ManagedMongoDB.UserKey}"] = "fake-database-admin";
        _cluster.SecretValues[$"apps/ledger-mongodb-users/{ManagedMongoDB.PasswordKey}"] = "fake-database-password";
    }

    Task Because() => Reconcile();

    [Fact]
    void should_apply_certificates_credentials_database_graph_connection_and_chronicle_in_that_order() =>
        string.Join(
            ',',
            _cluster.Applied.Select(resource => $"{resource.GetType().Name}:{resource.Metadata.Name}")).ShouldEqual(
            "V1Secret:ledger-chronicle-keystore," +
            "CertManagerIssuer:ledger-selfsigned," +
            "CertManagerCertificate:ledger-ca," +
            "CertManagerIssuer:ledger-ca," +
            "CertManagerCertificate:ledger-chronicle-tls," +
            "V1Secret:ledger-chronicle-credentials," +
            "PerconaServerMongoDB:ledger-mongodb," +
            "V1Secret:ledger-mongodb-connection," +
            "V1Secret:ledger-neo4j," +
            "V1Service:ledger-neo4j," +
            "V1StatefulSet:ledger-neo4j," +
            "V1Secret:ledger-chronicle-connection," +
            "V1Service:ledger-chronicle," +
            "V1StatefulSet:ledger-chronicle");
}
#endif
