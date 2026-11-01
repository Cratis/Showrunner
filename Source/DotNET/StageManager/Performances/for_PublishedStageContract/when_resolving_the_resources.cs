// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PublishedStageContract;

public class when_resolving_the_resources : Specification
{
    V1ResourceRequirements _defaults;
    V1ResourceRequirements _given;

    void Because()
    {
        _defaults = PublishedStageContract.Resources(PerformanceSamples.PublishedPerformance().Spec.Application.Published!);
        var published = PerformanceSamples.PublishedPerformance().Spec.Application.Published!;
        published.Resources.Limits["memory"] = "2Gi";
        _given = PublishedStageContract.Resources(published);
    }

    [Fact] void should_request_a_quarter_core_by_default() => _defaults.Requests["cpu"].ToString().ShouldEqual("250m");
    [Fact] void should_request_half_a_gibibyte_by_default() => _defaults.Requests["memory"].ToString().ShouldEqual("512Mi");
    [Fact] void should_limit_to_one_core_by_default() => _defaults.Limits["cpu"].ToString().ShouldEqual("1");
    [Fact] void should_limit_to_a_gibibyte_by_default() => _defaults.Limits["memory"].ToString().ShouldEqual("1Gi");
    [Fact] void should_use_a_given_value() => _given.Limits["memory"].ToString().ShouldEqual("2Gi");
    [Fact] void should_keep_the_other_defaults_beside_a_given_value() => _given.Limits["cpu"].ToString().ShouldEqual("1");
}
#endif
