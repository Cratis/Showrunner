// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// The message of the error the reporter hit, as the application raised it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ErrorMessage(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ErrorMessage"/>.
    /// </summary>
    public static readonly ErrorMessage NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="ErrorMessage"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ErrorMessage(string value) => new(value);
}
