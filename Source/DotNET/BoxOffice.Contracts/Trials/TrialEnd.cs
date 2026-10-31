// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Trials;

/// <summary>
/// The last day a trial runs. Direct ends it at midnight in the organization's time zone.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record TrialEnd(DateOnly Value) : ConceptAs<DateOnly>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="DateOnly"/> to a <see cref="TrialEnd"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator TrialEnd(DateOnly value) => new(value);
}
