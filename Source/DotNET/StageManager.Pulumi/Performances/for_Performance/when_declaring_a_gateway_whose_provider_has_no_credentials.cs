// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances.for_Performance;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_a_gateway_whose_provider_has_no_credentials : Specification
{
    Exception _exception;

    async Task Because() => _exception = await Catch.Exception(async () => await DeclaredResources.Run(() => _ = new Performance(
        "console",
        new PerformanceArgs
        {
            Namespace = PulumiSpecs.Namespace,
            AuthProxy = new AuthProxyArgs
            {
                Providers = [new AuthProxyProviderArgs { Kind = AuthProxyProviderKind.GitHub, Identity = "GitHub" }],
                Upstream = new AuthProxyUpstreamArgs { Url = "http://grafana/" },
            },
        })));

    [Fact] void should_be_refused() => _exception.ShouldNotBeNull();
    [Fact] void should_say_what_to_give_the_provider() => _exception.ToString().ShouldContain("ClientId and ClientSecret, or a CredentialsSecretName");
}
#endif
