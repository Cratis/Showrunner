// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_DesiredPerformanceBuilder;

public class when_building_without_authentication_or_onboarding : Specification
{
    DesiredPerformance _desired;

    void Because()
    {
        var performance = PerformanceSamples.Performance(onboarding: false, authentication: false);
        performance.Spec.Ingress.Enabled = false;
        _desired = DesiredPerformanceBuilder.Build(performance, new("chronicle-connection", "connectionString"), PerformanceSamples.Environment(), PerformanceSamples.Keys());
    }

    [Fact] void should_only_run_the_application() => _desired.Deployments.Single().Metadata.Name.ShouldEqual("crm-app");
    [Fact] void should_have_no_ingress() => _desired.Ingress.ShouldBeNull();
}
#endif
