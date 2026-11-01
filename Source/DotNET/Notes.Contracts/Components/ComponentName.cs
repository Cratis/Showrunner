// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Components;

/// <summary>
/// The name of a component the user picked, such as the name of a button, field or table.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ComponentName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ComponentName"/>.
    /// </summary>
    public static readonly ComponentName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ComponentName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ComponentName(string value) => new(value);
}
