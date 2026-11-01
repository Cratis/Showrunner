// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// How far a Deployment has come.
/// </summary>
/// <param name="Desired">The replicas wanted.</param>
/// <param name="Ready">The replicas that are ready.</param>
/// <param name="Updated">The replicas running the latest pod template.</param>
/// <param name="Total">The replicas that exist, old and new. Zero when unknown.</param>
/// <param name="Current">Whether the status describes the latest generation of the Deployment.</param>
public record DeploymentReadiness(int Desired, int Ready, int Updated, int Total = 0, bool Current = true)
{
    /// <summary>
    /// Gets a value indicating whether every wanted replica is ready and on the latest template, and no old replica is left.
    /// </summary>
    public bool IsReady => Current && Desired > 0 && Ready >= Desired && Updated >= Desired && Total <= Updated;
}
