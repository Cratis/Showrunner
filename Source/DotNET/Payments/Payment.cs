// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// A payment as the provider reports it.
/// </summary>
/// <param name="Id">The payment.</param>
/// <param name="Status">Where it stands.</param>
/// <param name="Amount">What it is for.</param>
/// <param name="Sequence">Which part of a recurring relationship it belongs to.</param>
/// <param name="CustomerId">The customer it was made for, when there is one.</param>
/// <param name="SubscriptionId">The subscription that generated it, when one did.</param>
/// <param name="MandateId">The permission to charge the customer again, when the payment gave or used one.</param>
/// <param name="CheckoutUrl">Where the customer pays, while it is open and was started for a customer to pay.</param>
/// <param name="Metadata">What the application attached when it asked for the payment.</param>
public record Payment(
    PaymentId Id,
    PaymentStatus Status,
    PaymentAmount Amount,
    PaymentSequence Sequence,
    CustomerId? CustomerId,
    SubscriptionId? SubscriptionId,
    string? MandateId,
    string? CheckoutUrl,
    IReadOnlyDictionary<string, string> Metadata);
