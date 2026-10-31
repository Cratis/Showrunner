// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Compute;

/// <summary>
/// Identifies a meter of the product, as the product declares it in <c>OfferingDeclared</c>.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record MeterId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="MeterId"/>.
    /// </summary>
    public static readonly MeterId NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="MeterId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator MeterId(string value) => new(value);
}
