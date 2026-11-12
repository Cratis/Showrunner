// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Audiences;

/// <summary>
/// Whom an audience decision is about. A grant is held at exactly one level, and a withdrawal removes the grant of its
/// own level only: withdrawing an audience from a user leaves the tenant's and the system's grants of it in place.
/// </summary>
public enum AudienceLevel
{
    /// <summary>
    /// The audience applies to everyone in one tenant.
    /// </summary>
    Tenant = 0,

    /// <summary>
    /// The audience applies to one user of one tenant.
    /// </summary>
    User = 1,

    /// <summary>
    /// The audience applies to the system itself and to every tenant of the product: automation and callers that are not
    /// a person.
    /// </summary>
    System = 2
}
