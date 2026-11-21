// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Understudies;

/// <summary>
/// How many backups the retention tiers keep. A tier left out keeps the operator's default: hourly 24, daily 7, weekly 4;
/// monthly and yearly are off.
/// </summary>
public sealed class UnderstudyRetentionArgs
{
    /// <summary>
    /// Gets the number of hours that keep their latest backup.
    /// </summary>
    public int? Hourly { get; init; }

    /// <summary>
    /// Gets the number of days that keep their latest backup.
    /// </summary>
    public int? Daily { get; init; }

    /// <summary>
    /// Gets the number of weeks that keep their latest backup.
    /// </summary>
    public int? Weekly { get; init; }

    /// <summary>
    /// Gets the number of months that keep their latest backup.
    /// </summary>
    public int? Monthly { get; init; }

    /// <summary>
    /// Gets the number of years that keep their latest backup.
    /// </summary>
    public int? Yearly { get; init; }

    /// <summary>
    /// Builds the spec of the retention.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("hourly", Hourly)
            .Put("daily", Daily)
            .Put("weekly", Weekly)
            .Put("monthly", Monthly)
            .Put("yearly", Yearly);
}
