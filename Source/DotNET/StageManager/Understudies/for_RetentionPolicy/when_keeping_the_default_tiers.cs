// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;

namespace Cratis.Showrunner.StageManager.Understudies.for_RetentionPolicy;

/// <summary>
/// An hourly backup for 40 days: hourly ×24, daily ×7 and weekly ×4 keep what they promise and nothing else, and nothing outlives the 35 day maximum.
/// </summary>
public class when_keeping_the_default_tiers : Specification
{
    static readonly DateTimeOffset _now = new(2026, 3, 15, 12, 30, 0, TimeSpan.Zero);

    List<DateTimeOffset> _takenAt;
    IReadOnlyList<RetentionVerdict> _verdicts;

    void Establish() => _takenAt = [.. Enumerable.Range(0, 40 * 24).Select(hours => new DateTimeOffset(2026, 3, 15, 12, 5, 0, TimeSpan.Zero).AddHours(-hours))];

    void Because() => _verdicts = new RetentionPolicy().Evaluate(_takenAt, _now);

    IEnumerable<DateTimeOffset> Kept(string tier) => _takenAt.Where((_, index) => _verdicts[index].Tiers.Contains(tier));

    [Fact] void should_keep_the_latest_backup_of_each_of_the_last_24_hours() => Kept(RetentionPolicy.HourlyTier).Count().ShouldEqual(24);
    [Fact] void should_keep_the_latest_backup_of_each_of_the_last_7_days() => Kept(RetentionPolicy.DailyTier).Count().ShouldEqual(7);
    [Fact] void should_keep_the_latest_backup_of_each_of_the_last_4_weeks() => Kept(RetentionPolicy.WeeklyTier).Count().ShouldEqual(4);
    [Fact] void should_not_keep_monthly_backups_by_default() => Kept(RetentionPolicy.MonthlyTier).ShouldBeEmpty();
    [Fact] void should_not_keep_yearly_backups_by_default() => Kept(RetentionPolicy.YearlyTier).ShouldBeEmpty();
    [Fact] void should_keep_the_newest_backup() => _verdicts[0].Keep.ShouldBeTrue();
    [Fact] void should_keep_the_newest_backup_in_every_tier_it_is_the_latest_of() => _verdicts[0].Tiers.ShouldContainOnly([RetentionPolicy.HourlyTier, RetentionPolicy.DailyTier, RetentionPolicy.WeeklyTier]);
    [Fact] void should_remove_a_backup_no_tier_keeps() => _verdicts[30].Keep.ShouldBeFalse();
    [Fact] void should_keep_far_fewer_than_all() => _verdicts.Count(verdict => verdict.Keep).ShouldBeLessThan(40);
}
#endif
