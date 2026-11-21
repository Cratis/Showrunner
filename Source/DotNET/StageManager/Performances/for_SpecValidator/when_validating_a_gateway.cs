// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_SpecValidator;

public class when_validating_a_gateway : Specification
{
    static IReadOnlyList<string> Validate(Action<PerformanceSpec> change)
    {
        var performance = ProxySamples.Gateway();
        change(performance.Spec);
        return SpecValidator.Validate(performance);
    }

    [Fact] void should_accept_a_gateway_without_application_chronicle_or_environment() => SpecValidator.Validate(ProxySamples.Gateway()).Count.ShouldEqual(0);
    [Fact] void should_require_the_environment_when_a_provider_takes_values_from_it() => Validate(spec => spec.AuthProxy.Providers[0].CredentialsSecretRef = null).Count.ShouldEqual(2);
    [Fact] void should_accept_the_environment_when_one_is_named() => Validate(spec => spec.Environment = new() { Name = "production", ConfigMapRef = new() { Name = "environment" } }).Count.ShouldEqual(0);
    [Fact] void should_refuse_an_upstream_that_is_not_a_url() => Validate(spec => spec.AuthProxy.Upstream!.Url = "grafana").Count.ShouldEqual(1);
    [Fact] void should_refuse_a_disabled_proxy() => Validate(spec => spec.AuthProxy.Enabled = false).Any(problem => problem.StartsWith("authProxy.upstream needs authProxy.enabled", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_onboarding() => Validate(spec => spec.Onboarding.Enabled = true).Any(problem => problem.StartsWith("authProxy.upstream and onboarding", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_a_chronicle() => Validate(spec => spec.Chronicle = new() { ConnectionSecretRef = new() { Name = "chronicle" } }).Count.ShouldEqual(1);
    [Fact] void should_refuse_an_application() => Validate(spec => spec.Application.Image = "registry.example.com/crm:1").Count.ShouldEqual(1);
}
#endif
