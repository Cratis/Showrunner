// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionFederationValidator;

public class when_federation_is_invalid : Specification
{
    IReadOnlyList<string> _portOfChronicle;
    IReadOnlyList<string> _hostedWithoutListener;
    IReadOnlyList<string> _hostedWithPlainHttp;
    IReadOnlyList<string> _dialWithoutAnything;
    IReadOnlyList<string> _valid;

    void Because()
    {
        _portOfChronicle = Validate(spec => spec.Federation = new() { Enabled = true, Port = 35000 });
        _hostedWithoutListener = Validate(spec => spec.Federation.Hosted = new() { Enabled = true, PublicEndpoint = "https://federation.example.com", TrustDomain = "example" });
        _hostedWithPlainHttp = Validate(spec => spec.Federation = new() { Enabled = true, Hosted = new() { Enabled = true, PublicEndpoint = "http://federation.example.com", TrustDomain = "example" } });
        _dialWithoutAnything = Validate(spec => spec.Federation.Dial = new() { Enabled = true });
        _valid = Validate(spec => spec.Federation.Dial = new() { Enabled = true, EventStore = "ledger", EnrollmentCodeSecretRef = new() { Name = "code" } });
    }

    static IReadOnlyList<string> Validate(Action<ProductionSpec> change)
    {
        var spec = ProductionSamples.External();
        change(spec);
        return ProductionValidator.Validate(spec);
    }

    [Fact] void should_refuse_a_port_chronicle_uses() => _portOfChronicle.Single().ShouldContain("federation.port");
    [Fact] void should_refuse_hosted_federation_without_the_listener() => _hostedWithoutListener.Single().ShouldContain("federation.hosted.enabled needs federation.enabled");
    [Fact] void should_refuse_a_public_endpoint_that_is_not_https() => _hostedWithPlainHttp.Single().ShouldContain("federation.hosted.publicEndpoint");
    [Fact] void should_require_an_event_store_and_an_enrollment_code_to_dial() => _dialWithoutAnything.Count.ShouldEqual(2);
    [Fact] void should_accept_a_dial_with_both() => _valid.ShouldBeEmpty();
}
#endif
