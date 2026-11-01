// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// What became of one version of a published application.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RolloutOutcome
{
    /// <summary>
    /// The version was ready and is the running version.
    /// </summary>
    RolledOut = 0,

    /// <summary>
    /// The previous version was restored because a new version failed.
    /// </summary>
    RolledBack = 1,

    /// <summary>
    /// The version was not ready in time.
    /// </summary>
    Failed = 2
}
