// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Suspensions;

/// <summary>
/// Whom a part suspension is about. A suspension is held at exactly one level and a resume ends the suspension of its own
/// level only: resuming a part for a user leaves the tenant's and the system's suspensions of it in place.
/// </summary>
public enum SuspensionLevel
{
    /// <summary>
    /// The part is suspended for everyone in one tenant.
    /// </summary>
    Tenant = 0,

    /// <summary>
    /// The part is suspended for one user of one tenant.
    /// </summary>
    User = 1,

    /// <summary>
    /// The part is suspended for the system caller: automation and callers that are not a person.
    /// </summary>
    System = 2
}
