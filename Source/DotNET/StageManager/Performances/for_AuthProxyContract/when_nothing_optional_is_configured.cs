// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_AuthProxyContract;

public class when_nothing_optional_is_configured : Specification
{
    IReadOnlyDictionary<string, string> _variables;

    void Because() => _variables = AuthProxyContract.EnvironmentVariables(PerformanceSamples.Performance(onboarding: false));

    [Fact] void should_not_resolve_tenants() => _variables.Keys.Any(key => key.Contains("TenantResolutions", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_not_require_claims() => _variables.Keys.Any(key => key.Contains("RequiredClaims", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_not_set_the_pages_path() => _variables.ContainsKey("Cratis__AuthProxy__PagesPath").ShouldBeFalse();
    [Fact] void should_not_set_the_key_ring_path() => _variables.ContainsKey("Cratis__AuthProxy__DataProtectionKeysPath").ShouldBeFalse();
    [Fact] void should_not_route_invitations_without_onboarding() => _variables.ContainsKey("Cratis__AuthProxy__Invite__Lobby__Backend__BaseUrl").ShouldBeFalse();
}
#endif
