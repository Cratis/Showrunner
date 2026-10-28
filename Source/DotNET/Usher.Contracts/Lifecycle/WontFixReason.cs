// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Lifecycle;

/// <summary>
/// Why support decided not to fix an issue, as the reporter is meant to read it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record WontFixReason(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="WontFixReason"/>.
    /// </summary>
    public static readonly WontFixReason NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="WontFixReason"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator WontFixReason(string value) => new(value);
}
