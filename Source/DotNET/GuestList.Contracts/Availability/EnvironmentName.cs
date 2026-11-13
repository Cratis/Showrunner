// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// The name of an environment a product runs in, such as Production or Staging, as the product configures it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record EnvironmentName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="EnvironmentName"/>.
    /// </summary>
    public static readonly EnvironmentName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="EnvironmentName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator EnvironmentName(string value) => new(value);
}
