// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// The module part of a <see cref="SliceLocation"/>: the top level of the application's slice hierarchy.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ModuleName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ModuleName"/>, used when the location has no module.
    /// </summary>
    public static readonly ModuleName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ModuleName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ModuleName(string value) => new(value);
}
