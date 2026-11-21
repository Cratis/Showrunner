// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// The agent harness whose worker Jobs the <see cref="Watchdog"/> cleans up, as a CronJob of its own: a worker Job that can never
/// start because a Secret its Pod mounts does not exist is deleted, behind a chain of fences. It only reports until cleanup is enabled.
/// </summary>
public sealed class WatchdogAgentHarnessArgs
{
    /// <summary>
    /// Gets the namespace of the agent workers.
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Gets a value indicating whether provably stuck Jobs are deleted. It defaults to <see langword="false"/>, which only reports them.
    /// </summary>
    public bool CleanupEnabled { get; init; }

    /// <summary>
    /// Gets the schedule of the job, or null for every five minutes.
    /// </summary>
    public string? Schedule { get; init; }
}
