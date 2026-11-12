// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// A payment the customer is sent to pay.
/// </summary>
/// <param name="Amount">What it is for.</param>
/// <param name="Description">What the customer sees on the payment.</param>
/// <param name="RedirectUrl">Where the customer returns to after paying.</param>
/// <param name="WebhookUrl">Where the provider reports that the payment changed.</param>
/// <param name="Metadata">What to keep on the payment, handed back on every read.</param>
/// <param name="CustomerId">The customer it is for; a first payment needs one to leave permission to charge again.</param>
public record PaymentRequest(
    PaymentAmount Amount,
    string Description,
    string RedirectUrl,
    string? WebhookUrl,
    IReadOnlyDictionary<string, string> Metadata,
    CustomerId? CustomerId = null);
