// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// The version of the application the reporter was running when the issue was reported, for example <c>4.12.1</c>.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ApplicationVersion(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ApplicationVersion"/>.
    /// </summary>
    public static readonly ApplicationVersion NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="ApplicationVersion"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ApplicationVersion(string value) => new(value);
}
