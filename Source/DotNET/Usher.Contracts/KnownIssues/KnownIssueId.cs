// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.KnownIssues;

/// <summary>
/// Identifies a known issue at the desk: the group the desk puts duplicate reports in, across tenants. Each report
/// stays its own issue in its own tenant; the known issue is the desk's, and the application only ever sees its id
/// and title.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record KnownIssueId(Guid Value) : ConceptAs<Guid>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="KnownIssueId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator KnownIssueId(Guid value) => new(value);
}
