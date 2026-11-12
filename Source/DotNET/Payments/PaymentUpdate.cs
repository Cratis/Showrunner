// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// A payment the provider says changed, read back from the provider so the application never trusts the callback itself.
/// </summary>
/// <param name="Payment">The payment as the provider has it now.</param>
/// <param name="Scope">What followed the webhook's route, when the application put something there; otherwise <see langword="null"/>.</param>
public record PaymentUpdate(Payment Payment, string? Scope);
