// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// The provider's identity of a payment.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record PaymentId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly convert from <see cref="string"/> to <see cref="PaymentId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator PaymentId(string value) => new(value);

    /// <summary>
    /// Implicitly convert from <see cref="PaymentId"/> to <see cref="string"/>.
    /// </summary>
    /// <param name="id">The identity to convert from.</param>
    public static implicit operator string(PaymentId id) => id.Value;
}
