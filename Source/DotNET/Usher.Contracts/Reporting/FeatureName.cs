// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// The feature part of a <see cref="SliceLocation"/>: the feature within the module.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record FeatureName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="FeatureName"/>, used when the location has no feature.
    /// </summary>
    public static readonly FeatureName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="FeatureName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator FeatureName(string value) => new(value);
}
