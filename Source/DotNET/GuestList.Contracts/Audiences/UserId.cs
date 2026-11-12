// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Audiences;

/// <summary>
/// Identifies a user of a tenant, as the product's identity provider names them. It is empty (<see cref="NotSet"/>) for a
/// decision at <see cref="AudienceLevel.System"/> or <see cref="AudienceLevel.Tenant"/>.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record UserId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="UserId"/>.
    /// </summary>
    public static readonly UserId NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="UserId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator UserId(string value) => new(value);
}
