// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// A subscription the provider charges on a schedule.
/// </summary>
/// <param name="Id">The subscription.</param>
/// <param name="Status">Whether it is still charging.</param>
public record RecurringSubscription(SubscriptionId Id, SubscriptionStatus Status);
