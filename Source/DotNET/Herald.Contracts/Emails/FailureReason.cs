// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Contracts.Emails;

/// <summary>
/// Why an email could not be delivered, in words safe to keep: it never holds a credential, a recipient address or a
/// value the email was filled with.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record FailureReason(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="FailureReason"/>.
    /// </summary>
    public static readonly FailureReason NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="FailureReason"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator FailureReason(string value) => new(value);
}
