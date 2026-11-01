// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Components;

/// <summary>
/// The part of a component the user picked, such as its header or a cell; NotSet when the component as a whole was picked.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ComponentPart(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ComponentPart"/>.
    /// </summary>
    public static readonly ComponentPart NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ComponentPart"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ComponentPart(string value) => new(value);
}
