// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// How a new version of an application replaces the running one.
/// </summary>
public enum RolloutStrategy
{
    /// <summary>
    /// The new pod starts before the old one stops.
    /// </summary>
    RollingUpdate = 0,

    /// <summary>
    /// The old pod stops first, so two never share state. Use it for an application that hosts a single-replica silo.
    /// </summary>
    Recreate = 1
}
