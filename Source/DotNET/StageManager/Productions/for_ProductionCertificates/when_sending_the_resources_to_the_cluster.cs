// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionCertificates;

public class when_sending_the_resources_to_the_cluster : Specification
{
    string _authority;
    string _certificate;
    string _database;

    void Because()
    {
        var production = ProductionSamples.Production(ProductionSamples.Managed());
        var resources = ProductionCertificates.Create(production);
        _authority = KubernetesJson.Serialize(resources[1]);
        _certificate = KubernetesJson.Serialize(resources[3]);
        _database = KubernetesJson.Serialize(ManagedMongoDB.Create(production));
    }

    [Fact] void should_send_the_api_version_of_cert_manager() => _certificate.ShouldContain("\"apiVersion\":\"cert-manager.io/v1\"");
    [Fact] void should_send_the_kind() => _certificate.ShouldContain("\"kind\":\"Certificate\"");
    [Fact] void should_send_the_spec_as_written() => _authority.ShouldContain("\"isCA\":true");
    [Fact] void should_send_the_keystore_request() => _certificate.ShouldContain("\"keystores\":{\"pkcs12\":{\"create\":true");
    [Fact] void should_send_no_status() => _certificate.Contains("\"status\"", StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_send_the_api_version_of_percona() => _database.ShouldContain("\"apiVersion\":\"psmdb.percona.com/v1\"");
    [Fact] void should_send_the_custom_resource_version() => _database.ShouldContain("\"crVersion\":\"1.18.0\"");
}
#endif
