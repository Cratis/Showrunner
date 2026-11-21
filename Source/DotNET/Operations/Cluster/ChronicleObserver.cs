// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// A Chronicle observer and its running state.
/// </summary>
/// <param name="Id">The observer id.</param>
/// <param name="RunningState">The numeric running state; <see cref="QuarantinedState"/> means quarantined.</param>
public sealed record ChronicleObserver(string Id, int RunningState)
{
    /// <summary>
    /// The running state of an observer that is quarantined and processes no events.
    /// </summary>
    public const int QuarantinedState = 5;

    /// <summary>
    /// Gets a value indicating whether the observer is quarantined.
    /// </summary>
    public bool IsQuarantined => RunningState == QuarantinedState;
}
