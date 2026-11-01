// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PublishedNetworkPolicy;

public class when_creating_the_policy : Specification
{
    V1NetworkPolicy _policy;

    void Because() => _policy = PublishedNetworkPolicy.Create(PerformanceSamples.PublishedPerformance());

    [Fact] void should_select_the_application_pods() => _policy.Spec.PodSelector.MatchLabels["app.kubernetes.io/name"].ShouldEqual("application");
    [Fact] void should_restrict_ingress() => _policy.Spec.PolicyTypes.Single().ShouldEqual("Ingress");
    [Fact] void should_allow_one_source() => _policy.Spec.Ingress.Single().FromProperty.Count.ShouldEqual(1);
    [Fact] void should_allow_only_authproxy_of_the_performance() => _policy.Spec.Ingress.Single().FromProperty.Single().PodSelector.MatchLabels["app.kubernetes.io/name"].ShouldEqual("authproxy");
    [Fact] void should_stay_within_the_performance() => _policy.Spec.Ingress.Single().FromProperty.Single().PodSelector.MatchLabels["showrunner.cratis.io/performance"].ShouldEqual("crm");
    [Fact] void should_allow_only_the_stage_port() => _policy.Spec.Ingress.Single().Ports.Single().Port.ToString().ShouldEqual("9090");
    [Fact] void should_be_owned_by_the_performance() => _policy.Metadata.OwnerReferences.Single().Name.ShouldEqual("crm");
}
#endif
