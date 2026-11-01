// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_SpecValidator;

public class when_validating_a_published_application : Specification
{
    IReadOnlyList<string> Validate(Action<PerformanceSpec> change)
    {
        var performance = PerformanceSamples.PublishedPerformance();
        change(performance.Spec);
        return SpecValidator.Validate(performance);
    }

    bool Mentions(IReadOnlyList<string> problems, string text) => problems.Any(problem => problem.Contains(text, StringComparison.Ordinal));

    static void DisableNotes(PerformanceSpec spec)
    {
        spec.Application.Published!.Notes!.Enabled = false;
        spec.Application.Published.Notes.CollectorUrl = string.Empty;
    }

    [Fact] void should_accept_a_complete_application() => Validate(_ => { }).Count.ShouldEqual(0);
    [Fact] void should_not_need_a_chronicle() => Validate(spec => spec.Chronicle = null).Count.ShouldEqual(0);
    [Fact] void should_require_a_version() => Mentions(Validate(spec => spec.Application.Published!.Version = string.Empty), "published.version").ShouldBeTrue();
    [Fact] void should_require_a_package() => Mentions(Validate(spec => spec.Application.Published!.PackageConfigMapRef.Name = string.Empty), "packageConfigMapRef").ShouldBeTrue();
    [Fact] void should_require_storage() => Mentions(Validate(spec => spec.Application.Published!.StorageConnectionSecretRef.Name = string.Empty), "storageConnectionSecretRef").ShouldBeTrue();
    [Fact] void should_require_authproxy() => Mentions(Validate(spec => spec.AuthProxy.Enabled = false), "authProxy.enabled").ShouldBeTrue();
    [Fact] void should_require_a_host() => Mentions(Validate(spec => spec.Ingress.Hosts.Clear()), "ingress.hosts").ShouldBeTrue();
    [Fact] void should_refuse_onboarding() => Mentions(Validate(spec => spec.Onboarding.Enabled = true), "onboarding is not supported").ShouldBeTrue();
    [Fact] void should_refuse_more_than_one_replica() => Mentions(Validate(spec => spec.Application.Replicas = 2), "one replica").ShouldBeTrue();
    [Fact] void should_refuse_a_rollout_timeout_that_is_not_positive() => Mentions(Validate(spec => spec.Application.Published!.RolloutTimeoutSeconds = 0), "rolloutTimeoutSeconds").ShouldBeTrue();
    [Fact] void should_require_a_trusted_proxy() => Mentions(Validate(spec => spec.Application.Published!.TrustedProxies.Clear()), "trustedProxies").ShouldBeTrue();
    [Fact] void should_accept_an_address_as_a_trusted_proxy() => Validate(spec => spec.Application.Published!.TrustedProxies = ["10.0.0.7", "::1"]).Count.ShouldEqual(0);
    [Fact] void should_refuse_a_trusted_proxy_that_is_not_an_address() => Mentions(Validate(spec => spec.Application.Published!.TrustedProxies = ["authproxy"]), "neither an IP address").ShouldBeTrue();
    [Fact] void should_require_an_absolute_collector_url() => Mentions(Validate(spec => spec.Application.Published!.Notes!.CollectorUrl = "collector"), "collectorUrl").ShouldBeTrue();
    [Fact] void should_require_a_collector_token() => Mentions(Validate(spec => spec.Application.Published!.Notes!.CollectorTokenSecretRef.Name = string.Empty), "collectorTokenSecretRef").ShouldBeTrue();
    [Fact] void should_require_a_publication_for_notes() => Mentions(Validate(spec => spec.Application.Published!.Notes!.PublicationId = string.Empty), "publicationId").ShouldBeTrue();
    [Fact] void should_require_an_application_name_for_notes() => Mentions(Validate(spec => spec.Application.Published!.Notes!.ApplicationName = string.Empty), "applicationName").ShouldBeTrue();
    [Fact] void should_not_check_notes_when_they_are_off() => Validate(DisableNotes).Count.ShouldEqual(0);
    [Fact] void should_accept_a_permanent_lifetime() => Validate(spec => spec.Lifetime = new() { Permanent = true }).Count.ShouldEqual(0);
    [Fact] void should_accept_a_lifetime_until_a_time() => Validate(spec => spec.Lifetime = new() { Until = DateTimeOffset.UtcNow.AddDays(1) }).Count.ShouldEqual(0);
    [Fact] void should_refuse_a_lifetime_that_is_both() => Mentions(Validate(spec => spec.Lifetime = new() { Permanent = true, Until = DateTimeOffset.UtcNow }), "exactly one").ShouldBeTrue();
    [Fact] void should_refuse_a_lifetime_that_is_neither() => Mentions(Validate(spec => spec.Lifetime = new()), "exactly one").ShouldBeTrue();
}
#endif
