// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Understudy.Contracts;

/// <summary>
/// Identifies one backup run. It is the location of the run in the backup catalog, <c>{production}/{eventStore}/{namespace}/{takenAt}</c>, so the same
/// backup always has the same id and a fact that is delivered twice can be recognized as one.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record BackupId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="BackupId"/>.
    /// </summary>
    public static readonly BackupId NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="BackupId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator BackupId(string value) => new(value);
}
