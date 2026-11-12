// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// How often a subscription charges.
/// </summary>
public enum PaymentInterval
{
    /// <summary>
    /// Every month.
    /// </summary>
    Monthly = 0,

    /// <summary>
    /// Every twelve months.
    /// </summary>
    Yearly = 1
}
