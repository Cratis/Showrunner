// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// The slice part of a <see cref="SliceLocation"/>: the behavior within the feature.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SliceName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="SliceName"/>, used when the location has no slice.
    /// </summary>
    public static readonly SliceName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="SliceName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator SliceName(string value) => new(value);
}
