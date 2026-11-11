// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_the_spec_is_invalid : a_preparation
{
    void Establish() => _performance.Spec.Chronicle = null;

    Task Because() => Prepare();

    [Fact] void should_not_prepare_the_performance() => _prepared.ShouldBeNull();
    [Fact] void should_be_blocked() => _status.Phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_record_only_the_invalid_spec() => Conditions.ShouldEqual("SpecValid=False/InvalidSpec");
    [Fact] void should_name_the_problem() => Condition("SpecValid").Message.ShouldEqual("Name the Chronicle to use with productionRef or chronicle.connectionSecretRef.");
    [Fact] void should_not_resolve_the_environment() => _resolver.Called.ShouldBeFalse();
}
#endif
