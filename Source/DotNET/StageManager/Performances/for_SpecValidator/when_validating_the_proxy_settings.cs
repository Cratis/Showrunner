// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_SpecValidator;

public class when_validating_the_proxy_settings : Specification
{
    static IReadOnlyList<string> Validate(Action<PerformanceSpec> change)
    {
        var performance = ProxySamples.ConfiguredApplication();
        change(performance.Spec);
        return SpecValidator.Validate(performance);
    }

    [Fact] void should_accept_every_optional_setting() => SpecValidator.Validate(ProxySamples.ConfiguredApplication()).Count.ShouldEqual(0);
    [Fact] void should_refuse_a_tenant_and_an_endpoint_together() => Validate(spec => spec.AuthProxy.Tenancy!.TenantId = "Default").Single().ShouldEqual("authProxy.tenancy takes tenantId or tenantsEndpoint, not both.");
    [Fact] void should_refuse_tenancy_that_names_neither() => Validate(spec => spec.AuthProxy.Tenancy = new()).Count.ShouldEqual(1);
    [Fact] void should_refuse_a_tenants_endpoint_that_is_not_a_url() => Validate(spec => spec.AuthProxy.Tenancy!.TenantsEndpoint = "tenants").Count.ShouldEqual(1);
    [Fact] void should_refuse_a_required_claim_without_a_name() => Validate(spec => spec.AuthProxy.RequiredClaims = [new() { AnyOf = ["acme"] }]).Count.ShouldEqual(1);
    [Fact] void should_refuse_an_empty_value_of_a_required_claim() => Validate(spec => spec.AuthProxy.RequiredClaims = [new() { Claim = "urn:github:team", AnyOf = [" "] }]).Count.ShouldEqual(1);
    [Fact] void should_accept_a_required_claim_that_only_has_to_be_present() => Validate(spec => spec.AuthProxy.RequiredClaims = [new() { Claim = "urn:github:team" }]).Count.ShouldEqual(0);
    [Fact] void should_refuse_pages_without_a_config_map() => Validate(spec => spec.AuthProxy.Pages = new()).Count.ShouldEqual(1);
    [Fact] void should_refuse_a_key_ring_without_a_claim() => Validate(spec => spec.AuthProxy.DataProtection = new()).Count.ShouldEqual(1);
    [Fact] void should_refuse_a_key_ring_with_several_replicas() => Validate(spec =>
    {
        spec.AuthProxy.DataProtection = new() { ClaimName = "keys" };
        spec.AuthProxy.Replicas = 2;
    }).Count.ShouldEqual(1);
    [Fact] void should_refuse_a_scope_with_a_comma() => Validate(spec => spec.AuthProxy.Providers[0].Scopes = ["read:user,user:email"]).Count.ShouldEqual(1);
    [Fact] void should_refuse_credentials_without_a_secret_name() => Validate(spec => spec.AuthProxy.Providers[0].CredentialsSecretRef = new()).Count.ShouldEqual(1);
    [Fact] void should_refuse_an_empty_ingress_annotation_name() => Validate(spec => spec.Ingress.Annotations[" "] = "x").Count.ShouldEqual(1);
}
#endif
