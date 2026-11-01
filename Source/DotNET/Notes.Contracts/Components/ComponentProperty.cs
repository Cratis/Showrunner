// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Components;

/// <summary>
/// The presentation property of a component the user suggests changing, such as its label or color.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ComponentProperty(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ComponentProperty"/>.
    /// </summary>
    public static readonly ComponentProperty NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ComponentProperty"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ComponentProperty(string value) => new(value);
}
