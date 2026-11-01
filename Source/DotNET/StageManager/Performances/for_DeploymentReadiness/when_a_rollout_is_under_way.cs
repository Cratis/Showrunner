// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_DeploymentReadiness;

public class when_a_rollout_is_under_way : Specification
{
    [Fact] void should_not_be_ready_while_the_old_pod_still_runs() => new DeploymentReadiness(1, 1, 1, 2).IsReady.ShouldBeFalse();
    [Fact] void should_be_ready_when_only_the_new_pod_is_left() => new DeploymentReadiness(1, 1, 1, 1).IsReady.ShouldBeTrue();
    [Fact] void should_not_be_ready_before_the_deployment_status_is_current() => new DeploymentReadiness(1, 1, 1, 1, false).IsReady.ShouldBeFalse();
    [Fact] void should_not_be_ready_without_a_ready_pod() => new DeploymentReadiness(1, 0, 1, 1).IsReady.ShouldBeFalse();
}
#endif
