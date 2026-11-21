// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// How a remedy may run.
/// </summary>
public enum RemedyPolicyMode
{
    /// <summary>
    /// Never - observe and report only.
    /// </summary>
    Never = 0,

    /// <summary>
    /// Only after a person reviewed it. Never satisfiable offline.
    /// </summary>
    Review = 1,

    /// <summary>
    /// Automatically, when evidence, budget and cooldown allow.
    /// </summary>
    Automatic = 2,
}
