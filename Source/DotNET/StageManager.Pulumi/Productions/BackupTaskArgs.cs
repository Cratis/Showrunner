// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// One schedule of Percona Backup for MongoDB: when a backup is taken and how many of them are kept.
/// </summary>
public sealed class BackupTaskArgs
{
    /// <summary>
    /// Gets the name of the schedule, such as <c>hourly</c>.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the cron expression the backups are taken on.
    /// </summary>
    public required string Schedule { get; init; }

    /// <summary>
    /// Gets how many backups of this schedule are kept.
    /// </summary>
    public int? Keep { get; init; }

    /// <summary>
    /// Builds the spec of the schedule.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec().Put("name", Name).Put("schedule", Schedule).Put("keep", Keep);
}
