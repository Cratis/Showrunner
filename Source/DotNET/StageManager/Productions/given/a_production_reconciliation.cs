// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Resources;

namespace Cratis.Showrunner.StageManager.Productions.given;

/// <summary>
/// The base of the Production reconciliation specifications: a Production on an external MongoDB in a cluster with cert-manager and the Percona operator,
/// where the database Secret exists and Chronicle's certificate is issued.
/// </summary>
public abstract class a_production_reconciliation : Specification
{
    protected const string CertificateAuthority = "-----BEGIN CERTIFICATE-----\nMIIBe2e\n-----END CERTIFICATE-----";
    protected const string DatabaseConnection = "mongodb://chronicle:e2e-database-password@mongo:27017";

    protected Production _production;
    protected FakeProductionCluster _cluster;
    protected ProductionPhase _phase;

    protected virtual ProductionSpec CreateSpec() => ProductionSamples.External();

    protected string StatusAsJson => JsonSerializer.Serialize(_production.Status);

    protected ResourceCondition Condition(string type) => _production.Status.Conditions.Single(condition => condition.Type == type);

    protected V1StatefulSet? Chronicle => _cluster.AppliedNamed<V1StatefulSet>("ledger-chronicle");

    protected V1Container ChronicleContainer => Chronicle!.Spec.Template.Spec.Containers.Single();

    protected V1EnvVar? Environment(string name) => ChronicleContainer.Env.SingleOrDefault(variable => variable.Name == name);

    void Establish()
    {
        _production = ProductionSamples.Production(CreateSpec());
        _cluster = new FakeProductionCluster();
        _cluster.SecretValues["apps/ledger-mongo/connectionString"] = DatabaseConnection;
        _cluster.SecretValues["apps/ledger-chronicle-tls/keystore.p12"] = "keystore";
        _cluster.SecretValues["apps/ledger-chronicle-tls/ca.crt"] = CertificateAuthority;
    }

    /// <summary>
    /// Reconciles the Production. Each specification calls it from its own Because().
    /// </summary>
    /// <returns>A task that completes when the Production is reconciled.</returns>
    protected async Task Reconcile()
    {
        var reconciler = new ProductionReconciler(_cluster, new ProductionPreparation(_cluster), TimeProvider.System, Substitute.For<ILogger<ProductionReconciler>>());
        _phase = await reconciler.ReconcileAsync(_production, CancellationToken.None);
    }
}
#endif
