// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// What an instance last observed of the check a remedy answers.
/// </summary>
public enum ObservedCheckState
{
    /// <summary>
    /// Nothing trustworthy is known - never observed, or the observation is not the remedy's verification check.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The check is failing.
    /// </summary>
    Failing = 1,

    /// <summary>
    /// The check is passing - there is nothing to remedy.
    /// </summary>
    Passing = 2,
}
