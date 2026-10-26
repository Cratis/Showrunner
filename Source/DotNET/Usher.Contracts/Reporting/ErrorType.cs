// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// The type of the error the reporter hit - for example the exception type name on the backend, or the error
/// name in the frontend.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ErrorType(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ErrorType"/>.
    /// </summary>
    public static readonly ErrorType NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="ErrorType"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ErrorType(string value) => new(value);
}
