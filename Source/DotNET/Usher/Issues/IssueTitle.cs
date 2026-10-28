// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Issues;

/// <summary>
/// What an issue is shown as: the summary of a problem described by hand, or the message of a reported error.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record IssueTitle(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="IssueTitle"/>.
    /// </summary>
    public static readonly IssueTitle NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="IssueTitle"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator IssueTitle(string value) => new(value);
}
