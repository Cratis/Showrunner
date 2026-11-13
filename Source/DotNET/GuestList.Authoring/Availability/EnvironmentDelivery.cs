// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring.Availability;

/// <summary>
/// How far the operator's decisions have reached one environment of a product.
/// </summary>
/// <param name="Environment">The environment.</param>
/// <param name="ReceivedThrough">When the newest decision the environment received was taken.</param>
/// <param name="LastReceivedAt">When the environment last told the operator it received a decision.</param>
public record EnvironmentDelivery(EnvironmentName Environment, DateTimeOffset ReceivedThrough, DateTimeOffset LastReceivedAt)
{
    /// <summary>
    /// Gets whether a decision has not reached the environment yet.
    /// </summary>
    /// <param name="decidedAt">When the operator took the decision.</param>
    /// <returns><see langword="true"/> when the environment has not told the operator it received it or a later one.</returns>
    public bool IsWaiting(DateTimeOffset decidedAt) => decidedAt > ReceivedThrough;
}
