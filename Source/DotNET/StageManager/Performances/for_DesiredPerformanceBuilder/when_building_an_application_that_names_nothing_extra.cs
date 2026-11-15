// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_DesiredPerformanceBuilder;

public class when_building_an_application_that_names_nothing_extra : Specification
{
    DesiredPerformance _desired;
    V1PodSpec _pod;
    V1Container _container;

    void Because()
    {
        _desired = DesiredPerformanceBuilder.Build(PerformanceSamples.Performance(onboarding: false, authentication: false), new("chronicle-connection", "connectionString"), PerformanceSamples.Environment(), PerformanceSamples.Keys());
        var application = _desired.Deployments.Single(deployment => deployment.Metadata.Name == "crm-app");
        _pod = application.Spec.Template.Spec;
        _container = _pod.Containers.Single();
    }

    [Fact] void should_keep_the_default_strategy() => _desired.Deployments.Single().Spec.Strategy.ShouldBeNull();
    [Fact] void should_check_the_port_for_readiness() => _container.ReadinessProbe.TcpSocket.ShouldNotBeNull();
    [Fact] void should_use_no_liveness_check() => _container.LivenessProbe.ShouldBeNull();
    [Fact] void should_use_no_startup_check() => _container.StartupProbe.ShouldBeNull();
    [Fact] void should_set_no_resources() => _container.Resources.ShouldBeNull();
    [Fact] void should_run_no_init_container() => _pod.InitContainers.ShouldBeNull();
    [Fact] void should_use_no_image_pull_secret() => _pod.ImagePullSecrets.ShouldBeNull();
    [Fact] void should_select_no_nodes() => _pod.NodeSelector.ShouldBeNull();
    [Fact] void should_have_no_egress_policy() => _desired.EgressPolicy.ShouldBeNull();
}
#endif
