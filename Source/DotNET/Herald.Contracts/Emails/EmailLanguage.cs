// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Contracts.Emails;

/// <summary>
/// The language an email was written in, as a culture name such as <c>en</c> or <c>nb-NO</c>. It is also the culture
/// dates in the email are formatted in.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record EmailLanguage(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="EmailLanguage"/>.
    /// </summary>
    public static readonly EmailLanguage NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="EmailLanguage"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator EmailLanguage(string value) => new(value);
}
