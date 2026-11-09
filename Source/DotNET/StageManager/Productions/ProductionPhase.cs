// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The phase a <see cref="Production"/> is in.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductionPhase
{
    /// <summary>
    /// Nothing has been decided yet.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Nothing is applied: the spec is invalid, asks for a change that is refused, or an input it names is missing.
    /// </summary>
    Blocked = 1,

    /// <summary>
    /// The resources are applied and not everything is ready yet.
    /// </summary>
    Progressing = 2,

    /// <summary>
    /// Chronicle, its database and its graph are ready.
    /// </summary>
    Ready = 3
}
