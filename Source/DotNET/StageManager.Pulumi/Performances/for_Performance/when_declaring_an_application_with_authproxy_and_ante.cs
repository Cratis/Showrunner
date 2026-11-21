// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;
using global::Pulumi;
using OperatorPerformance = Cratis.Showrunner.StageManager.Performances.Performance;
using OperatorPerformanceSpec = Cratis.Showrunner.StageManager.Performances.PerformanceSpec;
using OperatorSpecValidator = Cratis.Showrunner.StageManager.Performances.SpecValidator;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances.for_Performance;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_an_application_with_authproxy_and_ante : Specification
{
    DeclaredResources _declared;
    OperatorPerformanceSpec _spec;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Performance(
            "keldakraft",
            new PerformanceArgs
            {
                Namespace = PulumiSpecs.Namespace,
                ProductionName = "keldakraft",
                Application = new ApplicationArgs { Image = "ghcr.io/kelda/keldakraft:1.2.3", EventStores = ["KeldaKraft"] },
                Environment = new PerformanceEnvironmentArgs { Name = "production", ConfigMapName = "keldakraft-environment" },
                AuthProxy = new AuthProxyArgs
                {
                    Image = "docker.io/cratis/authproxy:2.30.5",
                    Providers =
                    [
                        new AuthProxyProviderArgs
                        {
                            Kind = AuthProxyProviderKind.GitHub,
                            Identity = "GitHub",
                            ClientId = "fake-client-id",
                            ClientSecret = "fake-client-secret-5d2c",
                            Scopes = ["read:user", "user:email", "read:org"],
                        },
                    ],
                    Tenancy = new AuthProxyTenancyArgs { TenantsEndpoint = "http://keldakraft/api/internal/tenants/selectable" },
                    RequiredClaims =
                    [
                        new RequiredClaimArgs { Claim = "urn:github:organization", AnyOf = ["kelda"] },
                        new RequiredClaimArgs { Claim = "urn:github:team", AnyOf = ["kelda/staff"] },
                    ],
                    Pages = new AuthProxyPagesArgs { Files = new Dictionary<string, Input<string>> { ["403.html"] = "<h1>Not allowed</h1>", ["styles.css"] = "body {}" } },
                    DataProtection = new AuthProxyDataProtectionArgs { Size = "1Gi" },
                    Resources = new ApplicationResourcesArgs { Requests = { ["cpu"] = "100m", ["memory"] = "128Mi" } },
                },
                Onboarding = new OnboardingArgs
                {
                    SignUp = SignUpMode.JoinTenant,
                    InvitationTypes = ["JoinTenant"],
                    Personas = ["Administrator", "Member"],
                    AllowedLocales = ["en", "nb"],
                    ReservedOrganizationNames = ["admin", "api"],
                    Legal = new LegalTermsArgs { TermsAndConditions = "# Terms", PrivacyPolicy = "# Privacy" },
                    AttestedProviders = [new AttestedProviderArgs { Key = "github", Issuer = "https://github.com", AcceptableAssurances = ["verified-email"] }],
                    Branding = new AnteBrandingArgs
                    {
                        LogoUrl = "/brand/logo.svg",
                        CustomCssUrl = "/brand/lobby.css",
                        PageTitle = "Kelda Kraft",
                        SplashMessage = "Loading Kelda Kraft",
                        Assets = new AnteBrandingAssetsArgs { Files = new Dictionary<string, Input<string>> { ["logo.svg"] = "<svg/>", ["lobby.css"] = "body {}" } },
                    },
                },
                Ingress = new IngressArgs
                {
                    ClassName = "nginx",
                    Hosts = ["app.keldakraft.example.com"],
                    TlsIssuerName = "letsencrypt-prod",
                    Annotations = { ["nginx.ingress.kubernetes.io/proxy-read-timeout"] = "3600" },
                },
                SigningKeys = new SigningKeysArgs { Rotation = 1, RetainPrevious = 3 },
            }));
        _spec = _declared.SpecOf<OperatorPerformanceSpec>("Performance");
    }

    [Fact] void should_turn_authproxy_on() => _spec.AuthProxy.Enabled.ShouldBeTrue();
    [Fact] void should_use_the_authproxy_image() => _spec.AuthProxy.Image.ShouldEqual("docker.io/cratis/authproxy:2.30.5");
    [Fact] void should_name_the_provider() => (_spec.AuthProxy.Providers.Single().Kind.ToString(), _spec.AuthProxy.Providers.Single().Identity).ShouldEqual(("GitHub", "GitHub"));
    [Fact] void should_ask_the_provider_for_its_scopes() => _spec.AuthProxy.Providers.Single().Scopes.ShouldContainOnly("read:user", "user:email", "read:org");
    [Fact] void should_refer_to_the_credentials_secret() => _spec.AuthProxy.Providers.Single().CredentialsSecretRef!.Name.ShouldEqual("keldakraft-github-credentials");
    [Fact] void should_write_the_client_id_to_the_secret() => _declared.StringData("keldakraft-github-credentials", "clientId").ShouldNotBeNull();
    [Fact] void should_write_the_client_secret_to_the_secret() => _declared.StringData("keldakraft-github-credentials", "clientSecret").ShouldNotBeNull();
    [Fact] void should_resolve_the_tenants_from_the_endpoint() => _spec.AuthProxy.Tenancy!.TenantsEndpoint.ShouldEqual("http://keldakraft/api/internal/tenants/selectable");
    [Fact] void should_require_the_organization_and_the_team() => _spec.AuthProxy.RequiredClaims.Select(claim => claim.Claim).ShouldContainOnly("urn:github:organization", "urn:github:team");
    [Fact] void should_accept_the_team_by_its_full_name() => _spec.AuthProxy.RequiredClaims.Single(claim => claim.Claim == "urn:github:team").AnyOf.ShouldContainOnly("kelda/staff");
    [Fact] void should_refer_to_the_pages_config_map() => _spec.AuthProxy.Pages!.ConfigMapRef.Name.ShouldEqual("keldakraft-authproxy-pages");
    [Fact] void should_write_the_pages_to_the_config_map() => _declared.ConfigMaps.Single(configMap => configMap.KubernetesName == "keldakraft-authproxy-pages").ShouldNotBeNull();
    [Fact] void should_keep_the_key_ring_on_a_claim_it_creates() => _spec.AuthProxy.DataProtection!.ClaimName.ShouldEqual("keldakraft-authproxy-keys");
    [Fact] void should_create_the_claim() => _declared.OfType("kubernetes:core/v1:PersistentVolumeClaim").Single().KubernetesName.ShouldEqual("keldakraft-authproxy-keys");
    [Fact] void should_request_the_resources_of_authproxy() => _spec.AuthProxy.Resources!.Requests["cpu"].ShouldEqual("100m");
    [Fact] void should_run_no_gateway() => _spec.AuthProxy.Upstream.ShouldBeNull();
    [Fact] void should_turn_ante_on() => _spec.Onboarding.Enabled.ShouldBeTrue();
    [Fact] void should_close_registration_to_invitations() => _spec.Onboarding.SignUp.ToString().ShouldEqual("JoinTenant");
    [Fact] void should_reserve_the_organization_names() => _spec.Onboarding.ReservedOrganizationNames.ShouldContainOnly("admin", "api");
    [Fact] void should_refer_to_the_legal_config_map() => _spec.Onboarding.Legal!.ConfigMapRef.Name.ShouldEqual("keldakraft-legal");
    [Fact] void should_accept_the_attested_provider() => _spec.Onboarding.AttestedProviders.Single().Key.ShouldEqual("github");
    [Fact] void should_brand_ante() => (_spec.Onboarding.Branding!.LogoUrl, _spec.Onboarding.Branding.PageTitle).ShouldEqual(("/brand/logo.svg", "Kelda Kraft"));
    [Fact] void should_serve_the_asset_files_by_name() => _spec.Onboarding.Branding!.Assets!.Files.ShouldContainOnly("lobby.css", "logo.svg");
    [Fact] void should_refer_to_the_brand_config_map() => _spec.Onboarding.Branding!.Assets!.ConfigMapRef.Name.ShouldEqual("keldakraft-brand");
    [Fact] void should_create_the_ingress_with_its_hosts() => _spec.Ingress.Hosts.ShouldContainOnly("app.keldakraft.example.com");
    [Fact] void should_ask_cert_manager_for_the_certificate() => _spec.Ingress.TlsIssuer!.Name.ShouldEqual("letsencrypt-prod");
    [Fact] void should_annotate_the_ingress() => _spec.Ingress.Annotations["nginx.ingress.kubernetes.io/proxy-read-timeout"].ShouldEqual("3600");
    [Fact] void should_rotate_the_signing_keys() => (_spec.SigningKeys.Rotation, _spec.SigningKeys.RetainPrevious).ShouldEqual((1, 3));
    [Fact] void should_be_valid_for_the_operator() => OperatorSpecValidator.Validate(new OperatorPerformance { Metadata = new() { Name = "keldakraft", NamespaceProperty = PulumiSpecs.Namespace }, Spec = _spec }).ShouldBeEmpty();
}
#endif
