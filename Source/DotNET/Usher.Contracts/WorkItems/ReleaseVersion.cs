// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.WorkItems;

/// <summary>
/// The version of the application a release shipped as, for example <c>4.12.3</c>; the reporter compares it with the
/// version they run.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ReleaseVersion(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ReleaseVersion"/>.
    /// </summary>
    public static readonly ReleaseVersion NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ReleaseVersion"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ReleaseVersion(string value) => new(value);
}
