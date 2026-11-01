// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Components;

/// <summary>
/// The id of a component the user picked, as the application's markup identifies it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ComponentId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ComponentId"/>.
    /// </summary>
    public static readonly ComponentId NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ComponentId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ComponentId(string value) => new(value);
}
