// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// The reporter's account of what happened, given when they describe an issue by hand.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record Description(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="Description"/>.
    /// </summary>
    public static readonly Description NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="Description"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator Description(string value) => new(value);
}
