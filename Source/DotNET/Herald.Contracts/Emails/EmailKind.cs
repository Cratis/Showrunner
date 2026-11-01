// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Contracts.Emails;

/// <summary>
/// The kind of an email, which is the name of its template: <c>invitation</c>, <c>welcome</c>, <c>sign-up-declined</c>.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record EmailKind(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="EmailKind"/>.
    /// </summary>
    public static readonly EmailKind NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="EmailKind"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator EmailKind(string value) => new(value);
}
