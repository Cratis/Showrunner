// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Activity;

/// <summary>
/// A number of sign-ins.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SignInCount(int Value) : ConceptAs<int>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="SignInCount"/>.
    /// </summary>
    public static readonly SignInCount NotSet = new(0);

    /// <summary>
    /// Implicitly converts a <see cref="int"/> to a <see cref="SignInCount"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator SignInCount(int value) => new(value);
}
