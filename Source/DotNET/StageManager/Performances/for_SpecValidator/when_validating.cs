// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_SpecValidator;

public class when_validating : Specification
{
    IReadOnlyList<string> Validate(Action<PerformanceSpec> change)
    {
        var performance = PerformanceSamples.Performance();
        change(performance.Spec);
        return SpecValidator.Validate(performance);
    }

    static void OnProduction(PerformanceSpec spec, string sink)
    {
        spec.ProductionRef = new() { Name = "ledger" };
        spec.Application.Sinks = [sink];
    }

    [Fact] void should_accept_a_complete_performance() => SpecValidator.Validate(PerformanceSamples.Performance()).Count.ShouldEqual(0);
    [Fact] void should_require_a_chronicle() => Validate(spec => spec.Chronicle = null).Count.ShouldEqual(1);
    [Fact] void should_require_an_image() => Validate(spec => spec.Application.Image = string.Empty).Count.ShouldEqual(1);
    [Fact] void should_require_authproxy_for_onboarding() => Validate(spec => spec.AuthProxy.Enabled = false).Any(problem => problem.Contains("authProxy.enabled", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_require_a_host_for_onboarding() => Validate(spec => spec.Ingress.Hosts.Clear()).Any(problem => problem.Contains("ingress.hosts", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_require_an_application_store_for_ante_to_observe() => Validate(spec => spec.Application.EventStores.Clear()).Any(problem => problem.Contains("eventStores", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_keep_antes_store_apart_from_the_applications() => Validate(spec => spec.Onboarding.EventStore = "Crm").Any(problem => problem.Contains("onboarding.eventStore", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_duplicate_provider_identities() => Validate(spec => spec.AuthProxy.Providers.Add(new() { Kind = AuthProxyProviderKind.Google, Identity = "github" })).Any(problem => problem.Contains("twice", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_require_a_production_for_sinks() => Validate(spec => spec.Application.Sinks = ["reporting"]).Any(problem => problem.Contains("application.sinks needs productionRef", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_an_empty_sink_name() => Validate(spec => OnProduction(spec, " ")).Any(problem => problem.Contains("empty name", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_require_a_production_for_the_graph() => Validate(spec => spec.Application.Graph = true).Any(problem => problem.Contains("application.graph needs productionRef", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_accept_sinks_on_a_production() => Validate(spec => OnProduction(spec, "reporting")).Count.ShouldEqual(0);
    [Fact] void should_refuse_a_provider_identity_with_odd_characters() => Validate(spec => spec.AuthProxy.Providers[0].Identity = "Entra/Id").Any(problem => problem.Contains("may only hold", StringComparison.Ordinal)).ShouldBeTrue();
}
#endif
