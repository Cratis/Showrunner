// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Resources;
using Cratis.Showrunner.StageManager.Understudies.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyStatusBuilder;

public class when_a_condition_does_not_change : Specification
{
    static readonly DateTimeOffset _first = new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);
    static readonly DateTimeOffset _later = _first.AddMinutes(10);

    Understudy _understudy;
    FixedTimeProvider _time;
    ResourceCondition _steady;
    ResourceCondition _changed;

    void Establish()
    {
        _understudy = UnderstudySamples.Understudy(UnderstudySamples.Spec());
        _understudy.Status = new();
        _time = new(_first);
    }

    void Because()
    {
        var first = new UnderstudyStatusBuilder(_understudy, _time);
        first.Set("Scheduled", true, "Scheduled", "ok");
        first.Set("BackupsHealthy", true, "Healthy", "ok");
        first.End(UnderstudyPhase.Healthy);
        first.Write();

        _time.Now = _later;
        var second = new UnderstudyStatusBuilder(_understudy, _time);
        second.Set("Scheduled", true, "Scheduled", "ok");
        second.Set("BackupsHealthy", false, "LatestRunFailed", "failed");
        second.End(UnderstudyPhase.Degraded);
        second.Write();

        _steady = _understudy.Status.Conditions.Single(condition => condition.Type == "Scheduled");
        _changed = _understudy.Status.Conditions.Single(condition => condition.Type == "BackupsHealthy");
    }

    [Fact] void should_keep_when_it_last_changed() => _steady.LastTransitionTime.ShouldEqual(_first);
    [Fact] void should_move_the_time_of_a_condition_that_changed() => _changed.LastTransitionTime.ShouldEqual(_later);
    [Fact] void should_write_the_phase() => _understudy.Status.Phase.ShouldEqual(UnderstudyPhase.Degraded);
    [Fact] void should_replace_a_condition_of_the_same_type() => _understudy.Status.Conditions.Count.ShouldEqual(2);
}
#endif
