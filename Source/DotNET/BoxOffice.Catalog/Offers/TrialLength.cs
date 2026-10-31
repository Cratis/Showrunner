// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// How many days a module can be tried before it is paid for; zero when it has no trial.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record TrialLength(int Value) : ConceptAs<int>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="TrialLength"/>.
    /// </summary>
    public static readonly TrialLength NotSet = new(0);

    /// <summary>
    /// Implicitly converts a <see cref="int"/> to a <see cref="TrialLength"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator TrialLength(int value) => new(value);
}
