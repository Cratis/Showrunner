// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Audiences;

/// <summary>
/// Names a module of a product, for an announcement aimed at the tenants that hold it. Marquee only carries the key;
/// what it means to hold a module is answered by an <see cref="IAudienceEntitlements"/> the host plugs in.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ModuleKey(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ModuleKey"/>.
    /// </summary>
    public static readonly ModuleKey NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ModuleKey"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ModuleKey(string value) => new(value);
}
