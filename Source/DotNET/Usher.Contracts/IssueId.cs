// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts;

/// <summary>
/// Identifies one reported issue. It is the event source id of every Usher event, private and public, in the
/// application and in the desk, and it never appears in an event payload. As an event source id it is the
/// lowercase, hyphenated <c>D</c> representation of the GUID.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record IssueId(Guid Value) : EventSourceId<Guid>(Value), IGeneratable<IssueId>
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to an <see cref="IssueId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator IssueId(Guid value) => new(value);

    /// <summary>
    /// Creates a new <see cref="IssueId"/>.
    /// </summary>
    /// <returns>A new <see cref="IssueId"/>.</returns>
    public static IssueId New() => new(GenerateValue.Uuid());
}
