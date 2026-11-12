// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// Whether a subscription is still charging.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>
    /// The subscription charges on schedule.
    /// </summary>
    Active = 0,

    /// <summary>
    /// The subscription was canceled and charges no more.
    /// </summary>
    Canceled = 1,

    /// <summary>
    /// The subscription is paused, for example because the customer's permission to charge ran out.
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// The subscription is in a state this contract does not name: not started yet or completed.
    /// </summary>
    Other = 3
}
