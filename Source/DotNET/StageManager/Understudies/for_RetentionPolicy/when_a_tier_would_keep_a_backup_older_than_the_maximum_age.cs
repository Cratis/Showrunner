// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;

namespace Cratis.Showrunner.StageManager.Understudies.for_RetentionPolicy;

public class when_a_tier_would_keep_a_backup_older_than_the_maximum_age : Specification
{
    static readonly DateTimeOffset _now = new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);

    IReadOnlyList<RetentionVerdict> _verdicts;

    void Because() => _verdicts = new RetentionPolicy(Hourly: 0, Daily: 0, Weekly: 0, Monthly: 12, Yearly: 5, MaxAgeDays: 35).Evaluate(
        [_now.AddDays(-1), _now.AddDays(-34), _now.AddDays(-36), _now.AddDays(-400)],
        _now);

    [Fact] void should_keep_a_young_backup() => _verdicts[0].Keep.ShouldBeTrue();
    [Fact] void should_keep_a_backup_just_inside_the_maximum_age() => _verdicts[1].Keep.ShouldBeTrue();
    [Fact] void should_not_keep_a_backup_older_than_the_maximum_age_even_when_a_month_would() => _verdicts[2].Keep.ShouldBeFalse();
    [Fact] void should_not_keep_a_yearly_backup_older_than_the_maximum_age() => _verdicts[3].Keep.ShouldBeFalse();
}
#endif
