// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// Where a payment stands, as every provider's own statuses reduce to it.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// The payment is not settled yet: waiting for the customer, or for the provider.
    /// </summary>
    Open = 0,

    /// <summary>
    /// The payment was paid.
    /// </summary>
    Paid = 1,

    /// <summary>
    /// The payment failed.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// The customer or the merchant canceled the payment.
    /// </summary>
    Canceled = 3,

    /// <summary>
    /// The customer did not pay in time.
    /// </summary>
    Expired = 4
}
