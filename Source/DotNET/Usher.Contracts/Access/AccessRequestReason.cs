// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Access;

/// <summary>
/// Why support asks the reporter for access to sign in as them, shown to the reporter with the request.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record AccessRequestReason(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="AccessRequestReason"/>.
    /// </summary>
    public static readonly AccessRequestReason NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="AccessRequestReason"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator AccessRequestReason(string value) => new(value);
}
