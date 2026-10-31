// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts;

namespace Cratis.Showrunner.BoxOffice.Compute;

/// <summary>
/// Identifies one unit of work that was admitted against a concurrency limit, from the moment it was admitted until it
/// ends.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ComputeWorkId(Guid Value) : ConceptAs<Guid>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="ComputeWorkId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ComputeWorkId(Guid value) => new(value);

    /// <summary>
    /// Creates the identity of a new unit of work.
    /// </summary>
    /// <returns>The identity.</returns>
    public static ComputeWorkId New() => new(Guid.NewGuid());
}
