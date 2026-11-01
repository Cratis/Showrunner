// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Context;

/// <summary>
/// The title of the dialog that was open when a note was given.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record DialogTitle(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="DialogTitle"/>.
    /// </summary>
    public static readonly DialogTitle NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="DialogTitle"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator DialogTitle(string value) => new(value);
}
