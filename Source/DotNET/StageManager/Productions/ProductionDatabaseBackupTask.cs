// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// One schedule of Percona Backup for MongoDB: when a backup is taken and how many of them are kept.
/// </summary>
public class ProductionDatabaseBackupTask
{
    /// <summary>
    /// Gets the schedules used when none are given.
    /// </summary>
    [Ignore]
    public static IReadOnlyList<ProductionDatabaseBackupTask> Defaults { get; } =
    [
        new() { Name = "hourly", Schedule = "0 * * * *", Keep = 24 },
        new() { Name = "daily", Schedule = "0 1 * * *", Keep = 7 },
        new() { Name = "weekly", Schedule = "0 2 * * 0", Keep = 4 },
        new() { Name = "monthly", Schedule = "0 3 1 * *", Keep = 12 }
    ];

    /// <summary>
    /// Gets or sets the name of the schedule.
    /// </summary>
    [Required]
    [Description("The name of the schedule, such as hourly.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cron schedule.
    /// </summary>
    [Required]
    [Description("The cron expression the backups are taken on.")]
    public string Schedule { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets how many backups of this schedule are kept.
    /// </summary>
    [Description("How many backups of this schedule are kept.")]
    public int Keep { get; set; }
}
