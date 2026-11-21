// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// The status of one container of a pod, init containers included.
/// </summary>
/// <param name="Name">The container name.</param>
/// <param name="Ready">Whether the container is ready.</param>
/// <param name="RestartCount">How often it has restarted.</param>
/// <param name="WaitingReason">Why it is waiting, when it is.</param>
/// <param name="TerminatedAt">When it last terminated, when it has.</param>
/// <param name="TerminationMessage">The message it terminated with.</param>
public sealed record ContainerInfo(
    string Name,
    bool Ready = true,
    int RestartCount = 0,
    string? WaitingReason = null,
    DateTimeOffset? TerminatedAt = null,
    string? TerminationMessage = null);
