// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Audiences;

/// <summary>
/// What kind of system a <see cref="SystemClientId"/> is. The client id identifies the system; the kind describes it, so an
/// operator can tell an MCP client from an API caller.
/// </summary>
public enum SystemKind
{
    /// <summary>
    /// No system is named: the level of the decision is not <see cref="AudienceLevel.NamedSystem"/>.
    /// </summary>
    None = 0,

    /// <summary>
    /// An MCP client.
    /// </summary>
    Mcp = 1,

    /// <summary>
    /// An API caller.
    /// </summary>
    Api = 2,

    /// <summary>
    /// Any other system, such as an integration or a service.
    /// </summary>
    Other = 3
}
