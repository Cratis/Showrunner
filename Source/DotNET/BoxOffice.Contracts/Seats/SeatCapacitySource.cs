// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Seats;

/// <summary>
/// Where a seat capacity comes from.
/// </summary>
public enum SeatCapacitySource
{
    /// <summary>
    /// An administrator granted the seats.
    /// </summary>
    Grant = 0,

    /// <summary>
    /// The organization bought the seats.
    /// </summary>
    Purchase = 1
}
