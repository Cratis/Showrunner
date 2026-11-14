// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// The outcome of redeeming an enrollment code.
/// </summary>
/// <param name="Outcome">How it went.</param>
/// <param name="Endpoint">The endpoint the credentials are for, when it was redeemed.</param>
public record DirectRedemptionResult(DirectRedemption Outcome, string Endpoint = "");
