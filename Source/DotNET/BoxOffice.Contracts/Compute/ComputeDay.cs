// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Compute;

/// <summary>
/// A day compute is metered for, in UTC.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ComputeDay(DateOnly Value) : ConceptAs<DateOnly>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="DateOnly"/> to a <see cref="ComputeDay"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ComputeDay(DateOnly value) => new(value);
}
