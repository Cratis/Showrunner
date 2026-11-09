// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_an_external_database_is_reconciled : a_production_reconciliation
{
    V1Service _service;
    string _connectionString;

    async Task Because()
    {
        await Reconcile();
        _service = _cluster.AppliedNamed<V1Service>("ledger-chronicle")!;
        _connectionString = _cluster.AppliedSecretValue("ledger-chronicle-connection", "connectionString")!;
    }

    [Fact] void should_be_progressing_until_chronicle_is_ready() => _phase.ShouldEqual(ProductionPhase.Progressing);
    [Fact] void should_run_the_production_image_of_the_version() => ChronicleContainer.Image.ShouldEqual("cratis/chronicle:19.148.0");
    [Fact] void should_run_one_replica() => Chronicle!.Spec.Replicas.ShouldEqual(1);
    [Fact] void should_have_a_headless_service() => _service.Spec.ClusterIP.ShouldEqual("None");
    [Fact] void should_name_the_service_port_chronicle_for_srv_discovery() => _service.Spec.Ports.Single(port => port.Port == 35000).Name.ShouldEqual("chronicle");
    [Fact] void should_govern_the_statefulset_with_the_headless_service() => Chronicle!.Spec.ServiceName.ShouldEqual("ledger-chronicle");
    [Fact] void should_create_no_database() => _cluster.AppliedOf<PerconaServerMongoDB>().ShouldBeEmpty();
    [Fact] void should_create_no_graph() => _cluster.AppliedNamed<V1StatefulSet>("ledger-neo4j").ShouldBeNull();
    [Fact] void should_issue_a_certificate_authority_and_a_certificate() => _cluster.AppliedOf<CertManagerCertificate>().Select(certificate => certificate.Metadata.Name).ShouldContainOnly(["ledger-ca", "ledger-chronicle-tls"]);
    [Fact] void should_have_a_self_signed_issuer_and_an_issuer_of_the_authority() => _cluster.AppliedOf<CertManagerIssuer>().Select(issuer => issuer.Metadata.Name).ShouldContainOnly(["ledger-selfsigned", "ledger-ca"]);
    [Fact] void should_write_chronicle_a_pkcs12_keystore() => _cluster.AppliedNamed<CertManagerCertificate>("ledger-chronicle-tls")!.Spec["keystores"]!["pkcs12"]!["create"]!.GetValue<bool>().ShouldBeTrue();
    [Fact] void should_cover_every_node_of_the_service_in_the_certificate() => _cluster.AppliedNamed<CertManagerCertificate>("ledger-chronicle-tls")!.Spec["dnsNames"]!.AsArray().Select(name => name!.GetValue<string>()).ShouldContain("*.ledger-chronicle.apps.svc.cluster.local");
    [Fact] void should_point_chronicle_at_the_keystore() => Environment("Cratis__Chronicle__Tls__CertificatePath")!.Value.ShouldEqual("/run/secrets/chronicle-tls/keystore.p12");
    [Fact] void should_give_chronicle_the_keystore_password_from_its_secret() => Environment("Cratis__Chronicle__Tls__CertificatePassword")!.ValueFrom.SecretKeyRef.Name.ShouldEqual("ledger-chronicle-keystore");
    [Fact] void should_bootstrap_the_client_of_the_applications_from_its_secret() => Environment("Cratis__Chronicle__Clients__0__ClientSecret")!.ValueFrom.SecretKeyRef.Name.ShouldEqual("ledger-chronicle-credentials");
    [Fact] void should_bootstrap_the_administrator_from_its_secret() => Environment("Cratis__Chronicle__Authentication__AdminUser__Password")!.ValueFrom.SecretKeyRef.Key.ShouldEqual("adminPassword");
    [Fact] void should_read_the_database_from_its_file() => Environment("Cratis__Chronicle__Storage__ConnectionDetailsFile")!.Value.ShouldEqual("/run/secrets/chronicle/Cratis__Chronicle__Storage__ConnectionDetails");
    [Fact] void should_mount_the_database_secret() => Chronicle!.Spec.Template.Spec.Volumes.Single(volume => volume.Name == "chronicle-connections").Projected.Sources.Single().Secret.Name.ShouldEqual("ledger-mongo");
    [Fact] void should_not_cluster_a_single_replica() => Environment("Cratis__Chronicle__Clustering__Type").ShouldBeNull();
    [Fact] void should_leave_the_neo4j_settings_unset() => ChronicleContainer.Env.Any(variable => variable.Name.Contains("Neo4j", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_write_no_secret_value_into_the_statefulset() => ChronicleContainer.Env.Any(variable => variable.Value?.Contains("password", StringComparison.OrdinalIgnoreCase) == true).ShouldBeFalse();
    [Fact] void should_connect_through_srv_with_the_client_credentials() => _connectionString.StartsWith("chronicle+srv://showrunner-applications:", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_connect_to_the_headless_service() => _connectionString.EndsWith("@ledger-chronicle.apps.svc.cluster.local", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_give_applications_the_certificate_authority() => _cluster.AppliedSecretValue("ledger-chronicle-connection", "ca.crt").ShouldEqual(CertificateAuthority);
    [Fact] void should_report_the_connection_secret() => _production.Status.ConnectionSecretName.ShouldEqual("ledger-chronicle-connection");
    [Fact] void should_report_the_address() => _production.Status.ChronicleAddress.ShouldEqual("chronicle+srv://ledger-chronicle.apps.svc.cluster.local");
    [Fact] void should_report_the_database_it_runs_with() => _production.Status.Database.ShouldEqual("MongoDB/External");
    [Fact] void should_own_everything_it_creates() => _cluster.Applied.TrueForAll(resource => resource.Metadata.OwnerReferences.Single().Kind == "Production").ShouldBeTrue();
    [Fact] void should_keep_the_client_secret_out_of_the_status() => StatusAsJson.Contains(_cluster.AppliedSecretValue("ledger-chronicle-credentials", "clientSecret")!, StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_write_the_status() => _cluster.StatusWritten.ShouldEqual(_production);
}
#endif
