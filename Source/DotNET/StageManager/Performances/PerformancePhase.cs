// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The phase a <see cref="Performance"/> is in.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PerformancePhase
{
    /// <summary>
    /// Nothing has been decided yet.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The Performance cannot roll out: the spec is invalid, the Chronicle connection or an environment value is missing.
    /// </summary>
    Blocked = 1,

    /// <summary>
    /// The workloads are applied and not yet ready.
    /// </summary>
    Progressing = 2,

    /// <summary>
    /// Every enabled component is ready.
    /// </summary>
    Ready = 3
}
