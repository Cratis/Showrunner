// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Entitlements;

/// <summary>
/// Identifies a module of the product, as the product declares it in <c>OfferingDeclared</c>.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ModuleId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ModuleId"/>.
    /// </summary>
    public static readonly ModuleId NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ModuleId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ModuleId(string value) => new(value);
}
