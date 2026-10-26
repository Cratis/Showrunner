// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// The name of the application an issue was reported in, as configured when the application adds Usher. A desk
/// serves several applications and uses it to tell them apart.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ApplicationName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ApplicationName"/>.
    /// </summary>
    public static readonly ApplicationName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="ApplicationName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ApplicationName(string value) => new(value);
}
