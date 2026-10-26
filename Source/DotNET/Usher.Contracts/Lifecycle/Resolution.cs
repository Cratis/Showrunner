// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Lifecycle;

/// <summary>
/// Support's note on how an issue was resolved, as the reporter is meant to read it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record Resolution(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="Resolution"/>.
    /// </summary>
    public static readonly Resolution NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="Resolution"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator Resolution(string value) => new(value);
}
