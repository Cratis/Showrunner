// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Entitlements;

/// <summary>
/// Where the seats a tenant may assign come from.
/// </summary>
public enum SeatCapacityOrigin
{
    /// <summary>
    /// The tenant's trial.
    /// </summary>
    Trial = 0,

    /// <summary>
    /// The operator granted them.
    /// </summary>
    Grant = 1,

    /// <summary>
    /// The tenant bought them.
    /// </summary>
    Purchase = 2
}
