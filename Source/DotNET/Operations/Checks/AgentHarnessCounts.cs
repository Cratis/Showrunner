// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// What one pass of the agent harness watchdog counted. Counts only: never API bodies, errors or credentials.
/// </summary>
public sealed record AgentHarnessCounts
{
    /// <summary>
    /// Gets how many worker Jobs were looked at.
    /// </summary>
    public int Considered { get; init; }

    /// <summary>
    /// Gets how many Jobs were left alone.
    /// </summary>
    public int Protected { get; init; }

    /// <summary>
    /// Gets how many things failed in a way that is known and safe (too many candidates, or a conflicting change).
    /// </summary>
    public int Failed { get; init; }

    /// <summary>
    /// Gets how many Jobs were deleted.
    /// </summary>
    public int Deleted { get; init; }

    /// <summary>
    /// Gets how many things could not be verified. Missing evidence is never permission to delete.
    /// </summary>
    public int Unknown { get; init; }

    /// <summary>
    /// Gets how many Jobs were provably stuck.
    /// </summary>
    public int Candidates { get; init; }

    /// <summary>
    /// Gets how many Jobs have a running Pod.
    /// </summary>
    public int Running { get; init; }

    /// <summary>
    /// Gets how many Jobs have a container that exited with a failing code.
    /// </summary>
    public int Exited { get; init; }
}
