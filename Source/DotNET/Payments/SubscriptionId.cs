// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// The provider's identity of a recurring subscription.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SubscriptionId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly convert from <see cref="string"/> to <see cref="SubscriptionId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator SubscriptionId(string value) => new(value);

    /// <summary>
    /// Implicitly convert from <see cref="SubscriptionId"/> to <see cref="string"/>.
    /// </summary>
    /// <param name="id">The identity to convert from.</param>
    public static implicit operator string(SubscriptionId id) => id.Value;
}
