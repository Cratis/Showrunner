// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// What a <see cref="Understudy"/> asks for. Schema only: the reconciliation that acts on it is a separate piece of work.
/// </summary>
public class UnderstudySpec
{
    /// <summary>
    /// Gets or sets the name of the Production to back up.
    /// </summary>
    [Required]
    [Description("The name of the Production to back up.")]
    public string ProductionRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cron schedule of the backups.
    /// </summary>
    [Description("The cron schedule of the backups.")]
    public string Schedule { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the longest a backup is kept, in days. A hard maximum, so a retention promise can hold.
    /// </summary>
    [Description("The longest a backup is kept, in days.")]
    public int MaxAgeDays { get; set; }

    /// <summary>
    /// Gets or sets the object storage the backups are written to, as a bucket URL.
    /// </summary>
    [Description("The object storage the backups are written to, as a bucket URL.")]
    public string StorageUrl { get; set; } = string.Empty;
}
