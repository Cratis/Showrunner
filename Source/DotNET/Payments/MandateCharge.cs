// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// A charge against the permission a customer gave with their first payment.
/// </summary>
/// <param name="CustomerId">The customer.</param>
/// <param name="Amount">What to charge.</param>
/// <param name="Description">What the customer sees on the payment.</param>
/// <param name="WebhookUrl">Where the provider reports that the payment changed.</param>
/// <param name="Metadata">What to keep on the payment, handed back on every read.</param>
public record MandateCharge(
    CustomerId CustomerId,
    PaymentAmount Amount,
    string Description,
    string? WebhookUrl,
    IReadOnlyDictionary<string, string> Metadata);
