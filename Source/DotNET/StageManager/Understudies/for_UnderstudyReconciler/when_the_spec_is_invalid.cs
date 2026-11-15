// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_the_spec_is_invalid : an_understudy_reconciliation
{
    protected override UnderstudySpec CreateSpec()
    {
        var spec = UnderstudySamples.Spec();
        spec.Schedule = "whenever";
        return spec;
    }

    async Task Because() => await Reconcile();

    [Fact] void should_block() => _phase.ShouldEqual(UnderstudyPhase.Blocked);
    [Fact] void should_say_the_spec_is_not_valid() => Condition(UnderstudyConditions.SpecValid).Status.ShouldEqual("False");
    [Fact] void should_say_why() => Condition(UnderstudyConditions.SpecValid).Reason.ShouldEqual("InvalidSchedule");
    [Fact] void should_schedule_nothing() => _cluster.Applied.ShouldBeEmpty();
    [Fact] void should_write_the_status() => _cluster.StatusWritten.ShouldBeSame(_understudy);
    [Fact] void should_report_the_phase_in_the_status() => _understudy.Status.Phase.ShouldEqual(UnderstudyPhase.Blocked);
    [Fact] void should_report_the_generation_it_describes() => _understudy.Status.ObservedGeneration.ShouldEqual(3);
}
#endif
