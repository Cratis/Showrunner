// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;
using OperatorPerformance = Cratis.Showrunner.StageManager.Performances.Performance;
using OperatorPerformanceSpec = Cratis.Showrunner.StageManager.Performances.PerformanceSpec;
using OperatorSpecValidator = Cratis.Showrunner.StageManager.Performances.SpecValidator;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances.for_Performance;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_a_gateway : Specification
{
    DeclaredResources _declared;
    OperatorPerformanceSpec _spec;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Performance(
            "keldakraft-console",
            new PerformanceArgs
            {
                Namespace = PulumiSpecs.Namespace,
                AuthProxy = new AuthProxyArgs
                {
                    Providers =
                    [
                        new AuthProxyProviderArgs
                        {
                            Kind = AuthProxyProviderKind.GitHub,
                            Identity = "GitHub",
                            CredentialsSecretName = "console-github",
                            Scopes = ["read:user", "user:email", "read:org"],
                        },
                    ],
                    Tenancy = new AuthProxyTenancyArgs { TenantId = "Default" },
                    RequiredClaims = [new RequiredClaimArgs { Claim = "urn:github:team", AnyOf = ["kelda/staff"] }],
                    Upstream = new AuthProxyUpstreamArgs { Url = "http://console-gateway.kubernetes-dashboard.svc.cluster.local/" },
                },
                Ingress = new IngressArgs { ClassName = "nginx", Hosts = ["console.keldakraft.example.com"], TlsIssuerName = "letsencrypt-prod" },
            }));
        _spec = _declared.SpecOf<OperatorPerformanceSpec>("Performance");
    }

    [Fact] void should_front_the_upstream() => _spec.AuthProxy.Upstream!.Url.ShouldEqual("http://console-gateway.kubernetes-dashboard.svc.cluster.local/");
    [Fact] void should_leave_the_default_for_a_third_party_upstream() => _spec.AuthProxy.Upstream!.ThirdParty.ShouldBeTrue();
    [Fact] void should_name_the_existing_credentials_secret() => _spec.AuthProxy.Providers.Single().CredentialsSecretRef!.Name.ShouldEqual("console-github");
    [Fact] void should_not_write_a_secret_for_an_existing_one() => _declared.Secrets.ShouldBeEmpty();
    [Fact] void should_not_use_chronicle() => (_spec.ProductionRef, _spec.Chronicle).ShouldEqual((null, null));
    [Fact] void should_write_no_environment_config_map() => _declared.ConfigMaps.ShouldBeEmpty();
    [Fact] void should_run_no_ante() => _spec.Onboarding.Enabled.ShouldBeFalse();
    [Fact] void should_be_valid_for_the_operator() => OperatorSpecValidator.Validate(new OperatorPerformance { Metadata = new() { Name = "keldakraft-console", NamespaceProperty = PulumiSpecs.Namespace }, Spec = _spec }).ShouldBeEmpty();
}
#endif
