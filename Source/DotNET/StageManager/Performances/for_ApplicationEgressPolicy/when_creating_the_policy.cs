// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_ApplicationEgressPolicy;

public class when_creating_the_policy : Specification
{
    V1NetworkPolicy _policy;
    V1NetworkPolicy _none;

    void Because()
    {
        var performance = PerformanceSamples.Performance();
        _none = ApplicationEgressPolicy.Create(performance);
        performance.Spec.Application.Egress =
        [
            new() { Ports = [587, 80] },
            new() { Cidr = "203.0.113.0/24", Ports = [7448] },
            new() { Namespace = "infrastructure", Ports = [35000] }
        ];
        _policy = ApplicationEgressPolicy.Create(performance)!;
    }

    [Fact] void should_create_none_without_rules() => _none.ShouldBeNull();
    [Fact] void should_select_the_application() => _policy.Spec.PodSelector.MatchLabels["app.kubernetes.io/name"].ShouldEqual("application");
    [Fact] void should_limit_egress_only() => _policy.Spec.PolicyTypes.ShouldContainOnly(["Egress"]);
    [Fact] void should_keep_dns_open() => _policy.Spec.Egress[0].Ports.All(port => port.Port.Value == "53").ShouldBeTrue();
    [Fact] void should_keep_the_namespace_open() => _policy.Spec.Egress[1].To.Single().PodSelector.ShouldNotBeNull();
    [Fact] void should_open_the_public_internet_without_the_private_ranges() => _policy.Spec.Egress[2].To.Single().IpBlock.Except.ShouldContainOnly(ApplicationEgressPolicy.PrivateRanges);
    [Fact] void should_open_the_ports_of_a_rule() => _policy.Spec.Egress[2].Ports.Select(port => port.Port.Value).ShouldContainOnly(["587", "80"]);
    [Fact] void should_open_an_address_range() => _policy.Spec.Egress[3].To.Single().IpBlock.Cidr.ShouldEqual("203.0.113.0/24");
    [Fact] void should_open_another_namespace() => _policy.Spec.Egress[4].To.Single().NamespaceSelector.MatchLabels["kubernetes.io/metadata.name"].ShouldEqual("infrastructure");
}
#endif
