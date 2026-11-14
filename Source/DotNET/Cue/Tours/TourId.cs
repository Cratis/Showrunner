// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Cue.Tours;

/// <summary>
/// The name of a guided tour, as the application knows it: a short, stable value such as <c>event-modeling</c>. It is what
/// the application's own tour is registered under and what is recorded when a person starts it, so renaming it makes
/// every person look as if they never started it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record TourId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="TourId"/>.
    /// </summary>
    public static readonly TourId NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="TourId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator TourId(string value) => new(value);
}
