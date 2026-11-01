// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Rehearsals;

/// <summary>
/// What a <see cref="Rehearsal"/> asks for. Schema only: the reconciliation that acts on it is a separate piece of work.
/// </summary>
public class RehearsalSpec
{
    /// <summary>
    /// Gets or sets the name of the Understudy whose backups are rehearsed.
    /// </summary>
    [Required]
    [Description("The name of the Understudy whose backups are rehearsed.")]
    public string UnderstudyRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cron schedule of the rehearsals.
    /// </summary>
    [Description("The cron schedule of the rehearsals.")]
    public string Schedule { get; set; } = string.Empty;
}
