// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.KnownIssues;

/// <summary>
/// The title of a known issue, as the reporter is meant to see it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record KnownIssueTitle(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="KnownIssueTitle"/>.
    /// </summary>
    public static readonly KnownIssueTitle NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="KnownIssueTitle"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator KnownIssueTitle(string value) => new(value);
}
