// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;
using global::Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances.for_Performance;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_pages_two_ways : Specification
{
    Exception _exception;

    async Task Because() => _exception = await Catch.Exception(async () => await DeclaredResources.Run(() => _ = new Performance(
        "console",
        new PerformanceArgs
        {
            Namespace = PulumiSpecs.Namespace,
            AuthProxy = new AuthProxyArgs
            {
                Providers = [new AuthProxyProviderArgs { Kind = AuthProxyProviderKind.GitHub, Identity = "GitHub", CredentialsSecretName = "console-github" }],
                Pages = new AuthProxyPagesArgs { ConfigMapName = "pages", Files = new Dictionary<string, Input<string>> { ["403.html"] = "x" } },
                Upstream = new AuthProxyUpstreamArgs { Url = "http://grafana/" },
            },
        })));

    [Fact] void should_be_refused() => _exception.ShouldNotBeNull();
    [Fact] void should_say_to_give_exactly_one() => _exception.ToString().ShouldContain("exactly one of ConfigMapName, Files and Directory");
}
#endif
