// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// Which part of a recurring relationship a payment belongs to.
/// </summary>
public enum PaymentSequence
{
    /// <summary>
    /// A single payment that sets nothing up for later.
    /// </summary>
    OneOff = 0,

    /// <summary>
    /// The customer's first payment, which also gives permission to charge them again.
    /// </summary>
    First = 1,

    /// <summary>
    /// A payment charged against permission the customer already gave.
    /// </summary>
    Recurring = 2
}
