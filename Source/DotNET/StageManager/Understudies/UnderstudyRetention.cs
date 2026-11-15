// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// How many backups the retention tiers keep.
/// </summary>
public class UnderstudyRetention
{
    /// <summary>
    /// Gets or sets the number of hours that keep their latest backup.
    /// </summary>
    [Description("The number of hours that keep their latest backup. Defaults to 24.")]
    public int Hourly { get; set; } = 24;

    /// <summary>
    /// Gets or sets the number of days that keep their latest backup.
    /// </summary>
    [Description("The number of days that keep their latest backup. Defaults to 7.")]
    public int Daily { get; set; } = 7;

    /// <summary>
    /// Gets or sets the number of weeks that keep their latest backup.
    /// </summary>
    [Description("The number of weeks that keep their latest backup. Defaults to 4.")]
    public int Weekly { get; set; } = 4;

    /// <summary>
    /// Gets or sets the number of months that keep their latest backup.
    /// </summary>
    [Description("The number of months that keep their latest backup. Off (0) by default.")]
    public int Monthly { get; set; }

    /// <summary>
    /// Gets or sets the number of years that keep their latest backup.
    /// </summary>
    [Description("The number of years that keep their latest backup. Off (0) by default.")]
    public int Yearly { get; set; }
}
