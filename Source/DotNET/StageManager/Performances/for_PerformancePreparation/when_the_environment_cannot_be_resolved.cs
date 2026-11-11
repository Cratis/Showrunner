// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_the_environment_cannot_be_resolved : a_preparation
{
    protected override EnvironmentResolution CreateResolution() => EnvironmentResolution.Failed("The file is not valid.");

    Task Because() => Prepare();

    [Fact] void should_not_prepare_the_performance() => _prepared.ShouldBeNull();
    [Fact] void should_be_blocked() => _status.Phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_record_the_failure() => Conditions.ShouldEqual("SpecValid=True/Valid | ChronicleConnection=True/Resolved | EnvironmentResolved=False/EnvironmentNotResolved");
    [Fact] void should_carry_the_resolver_failure() => Condition("EnvironmentResolved").Message.ShouldEqual("The file is not valid.");
}
#endif
