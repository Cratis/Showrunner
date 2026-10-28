// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// A stable hash identifying the same error across reports, so a desk can group duplicates: the same error in the
/// same slice and application version gives the same fingerprint. The application computes it; the desk treats it
/// as opaque.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ErrorFingerprint(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ErrorFingerprint"/>.
    /// </summary>
    public static readonly ErrorFingerprint NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="ErrorFingerprint"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ErrorFingerprint(string value) => new(value);
}
