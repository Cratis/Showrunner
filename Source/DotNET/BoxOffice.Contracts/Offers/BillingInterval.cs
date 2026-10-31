// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Offers;

/// <summary>
/// How often a price is charged.
/// </summary>
public enum BillingInterval
{
    /// <summary>
    /// Every month.
    /// </summary>
    Monthly = 0,

    /// <summary>
    /// Every year.
    /// </summary>
    Yearly = 1
}
