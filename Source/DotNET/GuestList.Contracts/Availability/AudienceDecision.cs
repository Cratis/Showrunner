// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// What the operator decided about an audience.
/// </summary>
public enum AudienceDecision
{
    /// <summary>
    /// The audience was granted.
    /// </summary>
    Granted = 0,

    /// <summary>
    /// The audience was withdrawn.
    /// </summary>
    Withdrawn = 1
}
