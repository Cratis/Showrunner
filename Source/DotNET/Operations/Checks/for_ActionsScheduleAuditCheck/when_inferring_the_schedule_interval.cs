// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Checks.for_ActionsScheduleAuditCheck;

public class when_inferring_the_schedule_interval : Specification
{
    double? _oddGaps;
    double? _evenGaps;
    double? _duplicates;
    double? _singleRun;
    double? _onlyTheLastTen;

    void Because()
    {
        // Gaps of 10, 20 and 100 seconds: the median is the middle one.
        _oddGaps = ActionsAuditWalk.ScheduleInterval([130, 120, 100, 0]);

        // Gaps of 10, 20, 30 and 100: the median is the mean of the two in the middle.
        _evenGaps = ActionsAuditWalk.ScheduleInterval([160, 60, 30, 10, 0]);

        // A rerun of a run shares its timestamp and must not shorten the interval.
        _duplicates = ActionsAuditWalk.ScheduleInterval([200, 200, 100, 100, 0]);
        _singleRun = ActionsAuditWalk.ScheduleInterval([100]);

        // Only the ten most recent runs count: the old, wildly different gaps are ignored.
        _onlyTheLastTen = ActionsAuditWalk.ScheduleInterval([.. Enumerable.Range(0, 10).Select(index => 1000.0 - (index * 60)), 10, 0]);
    }

    [Fact] void should_use_the_middle_gap_when_there_is_an_odd_number() => _oddGaps.ShouldEqual(20);
    [Fact] void should_use_the_mean_of_the_middle_gaps_when_there_is_an_even_number() => _evenGaps.ShouldEqual(25);
    [Fact] void should_ignore_duplicate_timestamps() => _duplicates.ShouldEqual(100);
    [Fact] void should_not_infer_an_interval_from_one_run() => _singleRun.ShouldBeNull();
    [Fact] void should_only_look_at_the_ten_most_recent_runs() => _onlyTheLastTen.ShouldEqual(60);
}
#endif
