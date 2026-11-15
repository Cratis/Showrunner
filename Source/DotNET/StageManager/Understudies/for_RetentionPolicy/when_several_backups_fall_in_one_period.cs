// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;

namespace Cratis.Showrunner.StageManager.Understudies.for_RetentionPolicy;

public class when_several_backups_fall_in_one_period : Specification
{
    static readonly DateTimeOffset _now = new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);

    IReadOnlyList<RetentionVerdict> _verdicts;

    /// <summary>
    /// Newest first: two in the same hour, then one in the previous hour, then an old one.
    /// </summary>
    void Because() => _verdicts = new RetentionPolicy(Hourly: 2, Daily: 0, Weekly: 0).Evaluate(
        [_now.AddMinutes(-10), _now.AddMinutes(-40), _now.AddMinutes(-70), _now.AddHours(-5)],
        _now);

    [Fact] void should_keep_the_latest_of_the_hour() => _verdicts[0].Keep.ShouldBeTrue();
    [Fact] void should_not_keep_the_earlier_one_of_the_same_hour() => _verdicts[1].Keep.ShouldBeFalse();
    [Fact] void should_keep_the_latest_of_the_previous_hour() => _verdicts[2].Keep.ShouldBeTrue();
    [Fact] void should_not_keep_what_the_tier_has_no_room_for() => _verdicts[3].Keep.ShouldBeFalse();
}
#endif
