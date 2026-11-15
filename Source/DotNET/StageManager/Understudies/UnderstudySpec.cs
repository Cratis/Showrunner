// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Understudies.Catalog;

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// What a <see cref="Understudy"/> asks for: how a Production is backed up, where the backups go and how long they are kept.
/// </summary>
public class UnderstudySpec
{
    /// <summary>
    /// The schedule of the database backups when none is given: every hour.
    /// </summary>
    public const string DefaultSchedule = "0 * * * *";

    /// <summary>
    /// Gets or sets the name of the Production to back up.
    /// </summary>
    [Required]
    [Description("The name of the Production to back up, in the namespace of the Understudy.")]
    public string ProductionRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cron schedule of the database backups.
    /// </summary>
    [Description("The cron schedule of the database backups. Defaults to every hour.")]
    public string Schedule { get; set; } = DefaultSchedule;

    /// <summary>
    /// Gets or sets the longest a backup is kept, in days. A hard maximum, so a retention promise can hold.
    /// </summary>
    [Description("The longest a backup is kept, in days, whatever the retention tiers say. Defaults to 35. It is also applied as an expiry rule on the bucket.")]
    public int MaxAgeDays { get; set; } = RetentionPolicy.DefaultMaxAgeDays;

    /// <summary>
    /// Gets or sets how many backups the retention tiers keep.
    /// </summary>
    [Description("How many backups the retention tiers keep: hourly 24, daily 7, weekly 4 by default; monthly and yearly are off.")]
    public UnderstudyRetention Retention { get; set; } = new();

    /// <summary>
    /// Gets or sets the prefix Chronicle was configured to give its databases.
    /// </summary>
    [Description("The prefix Chronicle was configured to give its databases, such as prod-. Empty by default.")]
    public string DatabasePrefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the databases of the Production that Chronicle does not own.
    /// </summary>
    [Description("Databases besides Chronicle's. A pattern with {eventStore} or {namespace}, such as {eventStore}+jobs+{namespace}, is taken per event store or namespace; a fixed name such as orleans once per Production. A database no rule claims is reported as Unclaimed.")]
    public IList<string> Databases { get; set; } = [];

    /// <summary>
    /// Gets or sets where the database backups are written.
    /// </summary>
    [Description("Where the database backups are written.")]
    public UnderstudyStorage Storage { get; set; } = new();

    /// <summary>
    /// Gets or sets how the keys of the Production are backed up. Absent means the keys are not backed up separately.
    /// </summary>
    [Description("How the keys of the Production's Vault are backed up. Needs a Production with a Vault. Absent means no separate key backups.")]
    public UnderstudyKeys? Keys { get; set; }

    /// <summary>
    /// Gets or sets the image of the job that takes the backups.
    /// </summary>
    [Description("The image of the job that takes the backups. Defaults to the Understudy image of the operator's release.")]
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the size of the scratch space a backup job dumps into before it uploads.
    /// </summary>
    [Description("The size of the scratch space a backup job dumps into before it uploads. It must hold the largest database. Defaults to 10Gi.")]
    public string WorkSize { get; set; } = "10Gi";

    /// <summary>
    /// Gets or sets whether the scheduled backups are paused.
    /// </summary>
    [Description("Pause the scheduled backups.")]
    public bool Suspend { get; set; }

    /// <summary>
    /// Gets the retention policy the spec asks for.
    /// </summary>
    /// <returns>The policy.</returns>
    public RetentionPolicy ToRetentionPolicy() => new(Retention.Hourly, Retention.Daily, Retention.Weekly, Retention.Monthly, Retention.Yearly, MaxAgeDays);
}
