// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Understudy.Contracts;

/// <summary>
/// The name of a Production, the Chronicle an Understudy backs up. It names a deployment, never a tenant or a person.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ProductionName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ProductionName"/>.
    /// </summary>
    public static readonly ProductionName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ProductionName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ProductionName(string value) => new(value);
}
