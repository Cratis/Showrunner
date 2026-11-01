// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Components;

/// <summary>
/// The value of a presentation property of a component, as text.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record PropertyValue(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="PropertyValue"/>.
    /// </summary>
    public static readonly PropertyValue NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="PropertyValue"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator PropertyValue(string value) => new(value);
}
