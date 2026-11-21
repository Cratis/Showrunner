// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// Percona Backup for MongoDB backups that the <see cref="Watchdog"/> expects to stay fresh.
/// </summary>
public sealed class WatchdogPbmArgs
{
    /// <summary>
    /// Gets the namespace of the backups.
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Gets how old the newest ready backup may be, or null for <c>3h</c>.
    /// </summary>
    public string? MaximumAge { get; init; }
}
