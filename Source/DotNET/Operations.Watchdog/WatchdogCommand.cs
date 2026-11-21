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
    /// <c>watchdog cluster</c>: one collection pass of the cluster checks and alert delivery.
    /// </summary>
    Cluster = 1,

    /// <summary>
    /// <c>watchdog test-alert</c>: a delivery test that changes no state and pings no heartbeat.
    /// </summary>
    TestAlert = 2,

    /// <summary>
    /// <c>watchdog runners</c>: stops hung jobs and replaces stuck runners of the GitHub Actions runner pool.
    /// </summary>
    Runners = 3,

    /// <summary>
    /// <c>watchdog agent-harness</c>: finds, and when enabled deletes, agent worker Jobs that can never start.
    /// </summary>
    AgentHarness = 4,

    /// <summary>
    /// <c>watchdog volumes</c>: detaches and deletes cloud volumes no PersistentVolume refers to any more.
    /// </summary>
    Volumes = 5,

    /// <summary>
    /// <c>watchdog probe</c>: the synthetic probe of the public endpoints, run from the cluster through the alert pipeline.
    /// </summary>
    Probe = 6,

    /// <summary>
    /// <c>watchdog probe-external</c>: the same probe run from outside the cluster, for example from CI, that needs no cluster access.
    /// </summary>
    ProbeExternal = 7
}
