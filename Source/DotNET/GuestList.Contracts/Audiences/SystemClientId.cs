// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Audiences;

/// <summary>
/// Identifies one system that calls a product, such as an integration, an MCP client or an API caller, by the client id it
/// authenticates with. It is empty (<see cref="NotSet"/>) for every level except <see cref="AudienceLevel.NamedSystem"/>.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SystemClientId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// The longest client id an audience decision accepts.
    /// </summary>
    public const int MaxLength = 256;

    /// <summary>
    /// Represents an unset <see cref="SystemClientId"/>.
    /// </summary>
    public static readonly SystemClientId NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="SystemClientId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator SystemClientId(string value) => new(value);
}
