// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// A CronJob that the <see cref="Watchdog"/> expects to keep succeeding, such as a database backup.
/// </summary>
public sealed class WatchdogCronJobArgs
{
    /// <summary>
    /// Gets the namespace of the CronJob, or null for the namespace of the watchdog.
    /// </summary>
    public string? Namespace { get; init; }

    /// <summary>
    /// Gets the name of the CronJob.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets how long it may go without a successful run, in the form <c>3h</c>, <c>30m</c> or <c>2d</c>.
    /// </summary>
    public required string MaximumAge { get; init; }

    /// <summary>
    /// Gets a value indicating whether it is fine for the CronJob not to exist.
    /// </summary>
    public bool Optional { get; init; }
}
