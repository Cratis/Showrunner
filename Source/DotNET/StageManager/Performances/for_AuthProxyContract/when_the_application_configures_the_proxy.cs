// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_AuthProxyContract;

public class when_the_application_configures_the_proxy : Specification
{
    IReadOnlyDictionary<string, string> _variables;
    IReadOnlyDictionary<string, string> _files;

    void Because()
    {
        var performance = ProxySamples.ConfiguredApplication();
        _variables = AuthProxyContract.EnvironmentVariables(performance);
        _files = AuthProxyContract.SecretFiles(performance, PerformanceSamples.Environment(), PerformanceSamples.Keys());
    }

    [Fact] void should_let_the_person_choose_among_the_tenants_of_the_endpoint() => _variables["Cratis__AuthProxy__TenantResolutions__0__Strategy"].ShouldEqual("Selection");
    [Fact] void should_name_the_tenants_endpoint() => _variables["Cratis__AuthProxy__TenantResolutions__0__Options__TenantsEndpoint"].ShouldEqual("http://crm-app/api/internal/tenants/selectable");
    [Fact] void should_not_name_a_fixed_tenant() => _variables.ContainsKey("Cratis__AuthProxy__TenantResolutions__0__Options__TenantId").ShouldBeFalse();
    [Fact] void should_still_forward_to_the_application() => _variables["Cratis__AuthProxy__Services__application__Backend__BaseUrl"].ShouldEqual("http://crm-app:8080/");
    [Fact] void should_still_ask_the_application_who_is_calling() => _variables.ContainsKey("Cratis__AuthProxy__Services__application__ResolveIdentityDetails").ShouldBeFalse();
    [Fact] void should_route_the_invitation_paths_to_ante_inside_the_cluster() => _variables["Cratis__AuthProxy__Invite__Lobby__Backend__BaseUrl"].ShouldEqual("http://crm-ante:8080/");
    [Fact] void should_point_at_the_custom_pages() => _variables["Cratis__AuthProxy__PagesPath"].ShouldEqual("/mnt/pages");
    [Fact] void should_not_point_at_a_key_ring_it_was_not_given() => _variables.ContainsKey("Cratis__AuthProxy__DataProtectionKeysPath").ShouldBeFalse();
    [Fact] void should_write_the_scopes_of_the_provider_that_names_them() => _files["authentication.providers.GitHub.scopes"].ShouldEqual("read:user,user:email");
    [Fact] void should_leave_the_scopes_of_the_other_provider_to_the_environment() => _files.ContainsKey("authentication.providers.EntraId.scopes").ShouldBeFalse();
}
#endif
