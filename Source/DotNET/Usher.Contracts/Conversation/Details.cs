// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Usher.Contracts.Conversation;

/// <summary>
/// Something the reporter adds to an issue of their own accord - what they were doing when an error happened, or
/// anything they want support to know later.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("Free text the reporter adds to an issue; it can name or identify them and is shown to support to work on the issue.")]
public record Details(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="Details"/>.
    /// </summary>
    public static readonly Details NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="Details"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator Details(string value) => new(value);
}
