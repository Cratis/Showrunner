// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Watchdog;

/// <summary>
/// What the job runner was asked to do.
/// </summary>
public enum WatchdogCommand
{
    /// <summary>
    /// The arguments did not name a command.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// <c>watchdog cluster</c>: one collection pass and alert delivery.
    /// </summary>
    Cluster = 1,

    /// <summary>
    /// <c>watchdog test-alert</c>: a delivery test that changes no state and pings no heartbeat.
    /// </summary>
    TestAlert = 2
}
