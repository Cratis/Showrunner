// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// Where an <see cref="Understudy"/> is.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UnderstudyPhase
{
    /// <summary>
    /// Nothing has been decided yet.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Nothing is scheduled: the spec is invalid or an input it names is missing.
    /// </summary>
    Blocked = 1,

    /// <summary>
    /// The backups are scheduled and no run has produced a backup yet.
    /// </summary>
    Scheduled = 2,

    /// <summary>
    /// The backups are scheduled and the latest run succeeded.
    /// </summary>
    Healthy = 3,

    /// <summary>
    /// The backups are scheduled, but the latest run failed or the key backups are refused.
    /// </summary>
    Degraded = 4
}
