// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// The correlation id of an Arc command or query that ran in the application around the time of a report, so the
/// desk can find the matching logs and traces. It is a value in the payload, not the correlation of the Usher event
/// itself, which is why it is Usher's own concept rather than <c>Cratis.Execution.CorrelationId</c>; the two convert
/// through their shared <see cref="Guid"/>.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record CorrelationId(Guid Value) : ConceptAs<Guid>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="CorrelationId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator CorrelationId(Guid value) => new(value);
}
