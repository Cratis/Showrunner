// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_federation_listens_as_the_hosted_authority : a_production_reconciliation
{
    const string Prefix = "Cratis__Chronicle__Federation__";

    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Federation.Enabled = true;
        spec.Federation.Hosted = new() { Enabled = true, PublicEndpoint = "https://federation.example.com:35100", TrustDomain = "example" };
        return spec;
    }

    void Establish() => _cluster.StatefulSetsReady = true;

    Task Because() => Reconcile();

    [Fact] void should_be_ready() => _phase.ShouldEqual(ProductionPhase.Ready);
    [Fact] void should_enable_the_listener() => Environment($"{Prefix}Enabled")!.Value.ShouldEqual("true");
    [Fact] void should_listen_on_the_default_port() => Environment($"{Prefix}Port")!.Value.ShouldEqual("35100");
    [Fact] void should_present_the_certificate_of_chronicle() => Environment($"{Prefix}CertificatePath")!.Value.ShouldEqual("/run/secrets/chronicle-tls/keystore.p12");
    [Fact] void should_read_the_password_from_a_file() => Environment($"{Prefix}CertificatePasswordPath")!.Value.ShouldEqual("/run/secrets/chronicle-federation/password");
    [Fact] void should_act_as_the_authority() => Environment($"{Prefix}Hosted__Enabled")!.Value.ShouldEqual("true");
    [Fact] void should_tell_the_authority_its_public_endpoint() => Environment($"{Prefix}Hosted__PublicEndpoint")!.Value.ShouldEqual("https://federation.example.com:35100");
    [Fact] void should_tell_the_authority_its_trust_domain() => Environment($"{Prefix}Hosted__TrustDomain")!.Value.ShouldEqual("example");
    [Fact] void should_open_the_container_port() => ChronicleContainer.Ports.Single(port => port.Name == "federation").ContainerPort.ShouldEqual(35100);
    [Fact] void should_expose_the_service_port() => _cluster.AppliedNamed<V1Service>("ledger-chronicle")!.Spec.Ports.Single(port => port.Name == "federation").Port.ShouldEqual(35100);
    [Fact] void should_add_the_public_host_to_the_certificate() => _cluster.AppliedNamed<CertManagerCertificate>("ledger-chronicle-tls")!.Spec!["dnsNames"]!.AsArray().Select(name => name!.GetValue<string>()).ShouldContain("federation.example.com");
    [Fact] void should_dial_nothing() => Environment($"{Prefix}Dial__Enabled").ShouldBeNull();
}
#endif
