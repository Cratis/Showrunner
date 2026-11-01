// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Contracts.Emails;

/// <summary>
/// Identifies one email Herald was asked to send. Every fact about the email carries it, so a host can follow one email
/// through being queued, retried and delivered even when many emails are about the same thing.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record EmailId(Guid Value) : ConceptAs<Guid>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="EmailId"/>.
    /// </summary>
    public static readonly EmailId NotSet = new(Guid.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to an <see cref="EmailId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator EmailId(Guid value) => new(value);

    /// <summary>
    /// Creates a new <see cref="EmailId"/>.
    /// </summary>
    /// <returns>The new identifier.</returns>
    public static EmailId New() => new(Guid.NewGuid());
}
