// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// An amount of money in a currency.
/// </summary>
/// <param name="Value">The amount, in the currency's major unit (euros, not cents).</param>
/// <param name="Currency">The three-letter currency code.</param>
public record PaymentAmount(decimal Value, string Currency);
