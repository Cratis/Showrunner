// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_AuthProxyContract;

public class when_the_upstream_is_a_cratis_service : Specification
{
    IReadOnlyDictionary<string, string> _variables;

    void Because() => _variables = AuthProxyContract.EnvironmentVariables(ProxySamples.Gateway(thirdParty: false));

    [Fact] void should_forward_to_the_upstream_as_the_frontend() => _variables["Cratis__AuthProxy__Services__application__Frontend__BaseUrl"].ShouldEqual(ProxySamples.UpstreamUrl);
    [Fact] void should_forward_to_the_upstream_as_the_backend() => _variables["Cratis__AuthProxy__Services__application__Backend__BaseUrl"].ShouldEqual(ProxySamples.UpstreamUrl);
    [Fact] void should_ask_the_upstream_who_is_calling() => _variables.ContainsKey("Cratis__AuthProxy__Services__application__ResolveIdentityDetails").ShouldBeFalse();
}
#endif
