// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_EnvironmentKeys;

public class when_listing_the_required_keys : Specification
{
    PerformanceSpec _spec;
    IReadOnlyList<string> _required;

    void Establish()
    {
        _spec = PerformanceSamples.Performance().Spec;
        _spec.AuthProxy.Providers.Add(new() { Kind = AuthProxyProviderKind.OpenId, Identity = "Partner" });
        _spec.AuthProxy.Providers.Add(new() { Kind = AuthProxyProviderKind.Apple, Identity = "Apple" });
    }

    void Because() => _required = EnvironmentKeys.RequiredBy(_spec);

    [Fact] void should_require_the_tenant_for_entra_id() => _required.Contains("authentication.providers.EntraId.tenantId").ShouldBeTrue();
    [Fact] void should_require_the_authority_for_open_id() => _required.Contains("authentication.providers.Partner.authority").ShouldBeTrue();
    [Fact] void should_not_require_an_authority_for_github() => _required.Contains("authentication.providers.GitHub.authority").ShouldBeFalse();
    [Fact] void should_require_client_credentials_for_apple() => _required.Contains("authentication.providers.Apple.clientSecret").ShouldBeTrue();
    [Fact] void should_require_the_mongodb_server_for_ante() => _required.Contains("ante.mongodb.server").ShouldBeTrue();
}
#endif
