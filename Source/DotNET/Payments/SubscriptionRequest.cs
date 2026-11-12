// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// A subscription to start.
/// </summary>
/// <param name="CustomerId">The customer to charge; they must have left permission with a first payment.</param>
/// <param name="Amount">What each period costs.</param>
/// <param name="Interval">How often to charge.</param>
/// <param name="Description">What the customer sees on every payment.</param>
/// <param name="WebhookUrl">Where the provider reports that one of its payments changed.</param>
/// <param name="Metadata">What to keep on the subscription and its payments.</param>
public record SubscriptionRequest(
    CustomerId CustomerId,
    PaymentAmount Amount,
    PaymentInterval Interval,
    string Description,
    string? WebhookUrl,
    IReadOnlyDictionary<string, string> Metadata);
