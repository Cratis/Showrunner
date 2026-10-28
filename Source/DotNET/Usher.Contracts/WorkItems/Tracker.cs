// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.WorkItems;

/// <summary>
/// The issue tracker a <see cref="WorkItem"/> lives in, as configured for the application at the desk - for example
/// <c>GitHub</c>.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record Tracker(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="Tracker"/>.
    /// </summary>
    public static readonly Tracker NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="Tracker"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator Tracker(string value) => new(value);
}
