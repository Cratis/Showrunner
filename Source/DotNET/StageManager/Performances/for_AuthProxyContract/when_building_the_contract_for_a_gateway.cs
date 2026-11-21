// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_AuthProxyContract;

public class when_building_the_contract_for_a_gateway : Specification
{
    IReadOnlyDictionary<string, string> _variables;
    IReadOnlyDictionary<string, string> _files;

    void Because()
    {
        var gateway = ProxySamples.Gateway();
        _variables = AuthProxyContract.EnvironmentVariables(gateway);
        _files = AuthProxyContract.SecretFiles(gateway, ProxySamples.CredentialsEnvironment(), PerformanceSamples.Keys());
    }

    [Fact] void should_forward_to_the_upstream_as_the_frontend() => _variables["Cratis__AuthProxy__Services__application__Frontend__BaseUrl"].ShouldEqual(ProxySamples.UpstreamUrl);
    [Fact] void should_not_call_the_upstream_to_ask_who_is_calling() => _variables["Cratis__AuthProxy__Services__application__ResolveIdentityDetails"].ShouldEqual("false");
    [Fact] void should_give_the_upstream_no_backend() => _variables.ContainsKey("Cratis__AuthProxy__Services__application__Backend__BaseUrl").ShouldBeFalse();
    [Fact] void should_not_name_an_environment_it_does_not_have() => _variables.ContainsKey("Cratis__AuthProxy__Application__EnvironmentName").ShouldBeFalse();
    [Fact] void should_not_turn_onboarding_on() => _variables["Cratis__AuthProxy__Application__Onboarding"].ShouldEqual("false");
    [Fact] void should_resolve_the_tenant_to_the_fixed_one() => _variables["Cratis__AuthProxy__TenantResolutions__0__Strategy"].ShouldEqual("Default");
    [Fact] void should_name_the_fixed_tenant() => _variables["Cratis__AuthProxy__TenantResolutions__0__Options__TenantId"].ShouldEqual("Default");
    [Fact] void should_require_the_first_claim() => _variables["Cratis__AuthProxy__Authorization__RequiredClaims__0__Claim"].ShouldEqual("urn:github:organization");
    [Fact] void should_require_one_of_the_values_of_the_first_claim() => _variables["Cratis__AuthProxy__Authorization__RequiredClaims__0__AnyOf__0"].ShouldEqual("acme");
    [Fact] void should_require_the_second_claim() => _variables["Cratis__AuthProxy__Authorization__RequiredClaims__1__Claim"].ShouldEqual("urn:github:team");
    [Fact] void should_accept_the_second_value_of_the_second_claim() => _variables["Cratis__AuthProxy__Authorization__RequiredClaims__1__AnyOf__1"].ShouldEqual("acme/ops");
    [Fact] void should_point_at_the_custom_pages() => _variables["Cratis__AuthProxy__PagesPath"].ShouldEqual(AuthProxyContract.PagesDirectory);
    [Fact] void should_point_at_the_key_ring() => _variables["Cratis__AuthProxy__DataProtectionKeysPath"].ShouldEqual(AuthProxyContract.KeysDirectory);
    [Fact] void should_not_carry_the_client_secret_as_a_variable() => _variables.Values.Any(value => value.Contains(ProxySamples.GitHubClientSecret)).ShouldBeFalse();
    [Fact] void should_write_the_client_secret_as_a_file() => _files["authentication.providers.GitHub.clientSecret"].ShouldEqual(ProxySamples.GitHubClientSecret);
    [Fact] void should_write_the_scopes_of_the_provider_as_a_file() => _files["authentication.providers.GitHub.scopes"].ShouldEqual("read:user,user:email,read:org");
    [Fact] void should_not_write_onboarding_files() => _files.Keys.Any(key => key.StartsWith("onboarding.", StringComparison.Ordinal)).ShouldBeFalse();
}
#endif
