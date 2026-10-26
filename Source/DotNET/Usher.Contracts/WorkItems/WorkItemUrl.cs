// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.WorkItems;

/// <summary>
/// The address the reporter can open a work item at in its tracker. It is <see cref="NotSet"/> when the tracker is
/// not reachable for the reporter, for example a private repository.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record WorkItemUrl(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="WorkItemUrl"/>.
    /// </summary>
    public static readonly WorkItemUrl NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="WorkItemUrl"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator WorkItemUrl(string value) => new(value);
}
