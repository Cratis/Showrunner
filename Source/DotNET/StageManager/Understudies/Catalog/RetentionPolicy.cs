// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Cratis.Showrunner.StageManager.Understudies.Catalog;

/// <summary>
/// How long backups are kept: how many of the most recent hours, days, weeks, months and years keep their latest backup, and a hard maximum age no tier can extend.
/// </summary>
/// <param name="Hourly">The number of hours that keep their latest backup. Defaults to 24.</param>
/// <param name="Daily">The number of days that keep their latest backup. Defaults to 7.</param>
/// <param name="Weekly">The number of weeks that keep their latest backup. Defaults to 4.</param>
/// <param name="Monthly">The number of months that keep their latest backup. Off by default.</param>
/// <param name="Yearly">The number of years that keep their latest backup. Off by default.</param>
/// <param name="MaxAgeDays">The longest a backup is kept, in days, whatever the tiers say. Defaults to 35.</param>
public record RetentionPolicy(int Hourly = 24, int Daily = 7, int Weekly = 4, int Monthly = 0, int Yearly = 0, int MaxAgeDays = RetentionPolicy.DefaultMaxAgeDays)
{
    /// <summary>
    /// The hard maximum age when none is set.
    /// </summary>
    public const int DefaultMaxAgeDays = 35;

    /// <summary>
    /// The name of the hourly tier.
    /// </summary>
    public const string HourlyTier = "hourly";

    /// <summary>
    /// The name of the daily tier.
    /// </summary>
    public const string DailyTier = "daily";

    /// <summary>
    /// The name of the weekly tier.
    /// </summary>
    public const string WeeklyTier = "weekly";

    /// <summary>
    /// The name of the monthly tier.
    /// </summary>
    public const string MonthlyTier = "monthly";

    /// <summary>
    /// The name of the yearly tier.
    /// </summary>
    public const string YearlyTier = "yearly";

    /// <summary>
    /// Decides which of the backups of one scope are kept and by which tiers. A backup is kept when it is the latest of one of the most recent hours, days, weeks,
    /// months or years that have any, and it is younger than the maximum age. Nothing older than the maximum age is kept, whatever the tiers say.
    /// </summary>
    /// <param name="takenAt">When each backup was taken.</param>
    /// <param name="now">The current time.</param>
    /// <returns>One verdict per backup, in the order given.</returns>
    public IReadOnlyList<RetentionVerdict> Evaluate(IReadOnlyList<DateTimeOffset> takenAt, DateTimeOffset now)
    {
        var oldest = now.AddDays(-MaxAgeDays);
        var tiers = takenAt.Select(_ => new List<string>()).ToList();
        var candidates = Enumerable.Range(0, takenAt.Count).Where(index => takenAt[index] > oldest).ToList();

        Apply(HourlyTier, Hourly, candidates, takenAt, tiers, moment => moment.UtcDateTime.ToString("yyyyMMddHH", CultureInfo.InvariantCulture));
        Apply(DailyTier, Daily, candidates, takenAt, tiers, moment => moment.UtcDateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
        Apply(WeeklyTier, Weekly, candidates, takenAt, tiers, WeekOf);
        Apply(MonthlyTier, Monthly, candidates, takenAt, tiers, moment => moment.UtcDateTime.ToString("yyyyMM", CultureInfo.InvariantCulture));
        Apply(YearlyTier, Yearly, candidates, takenAt, tiers, moment => moment.UtcDateTime.ToString("yyyy", CultureInfo.InvariantCulture));

        return [.. tiers.Select(kept => new RetentionVerdict(kept.Count > 0, kept))];
    }

    static string WeekOf(DateTimeOffset moment)
    {
        var date = moment.UtcDateTime.Date;
        var monday = date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
        return monday.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
    }

    static void Apply(string tier, int count, List<int> candidates, IReadOnlyList<DateTimeOffset> takenAt, List<List<string>> tiers, Func<DateTimeOffset, string> bucketOf)
    {
        if (count <= 0)
        {
            return;
        }

        var latestPerBucket = candidates
            .GroupBy(index => bucketOf(takenAt[index]), StringComparer.Ordinal)
            .Select(group => (Bucket: group.Key, Latest: group.OrderByDescending(index => takenAt[index]).First()))
            .OrderByDescending(entry => entry.Bucket, StringComparer.Ordinal)
            .Take(count);

        foreach (var entry in latestPerBucket)
        {
            tiers[entry.Latest].Add(tier);
        }
    }
}
