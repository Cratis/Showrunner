// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Understudy.Contracts;

/// <summary>
/// Identifies one operation of Stage Manager: a backup job, a restore or a rehearsal. It is the name of the resource or job that did the work,
/// so the same operation always has the same id and a fact that is delivered twice can be recognized as one.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record OperationId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="OperationId"/>.
    /// </summary>
    public static readonly OperationId NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="OperationId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator OperationId(string value) => new(value);
}
