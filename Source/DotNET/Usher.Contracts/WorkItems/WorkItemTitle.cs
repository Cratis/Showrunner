// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.WorkItems;

/// <summary>
/// The title of a work item in its tracker.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record WorkItemTitle(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="WorkItemTitle"/>.
    /// </summary>
    public static readonly WorkItemTitle NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="WorkItemTitle"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator WorkItemTitle(string value) => new(value);
}
