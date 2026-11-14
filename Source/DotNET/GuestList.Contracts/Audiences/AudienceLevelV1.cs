// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Audiences;

/// <summary>
/// The levels generation 1 of the audience events knew. It exists so the generation 1 records keep the schema they shipped
/// with; do not use it anywhere else.
/// </summary>
public enum AudienceLevelV1
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
    /// The audience applies to every system of the product.
    /// </summary>
    System = 2
}
