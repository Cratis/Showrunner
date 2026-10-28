// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.WorkItems;

/// <summary>
/// The tracker's own reference to a work item, for example <c>Cratis/Showrunner#42</c>. It identifies the work item
/// within an issue's links, and the release that fixes an issue names the work item it closed by it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record WorkItemReference(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="WorkItemReference"/>.
    /// </summary>
    public static readonly WorkItemReference NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="WorkItemReference"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator WorkItemReference(string value) => new(value);
}
