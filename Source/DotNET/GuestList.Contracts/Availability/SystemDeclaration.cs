// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Audiences;

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// A system that calls the product, such as an integration, an MCP client or an API caller, as the product knows it.
/// </summary>
/// <param name="ClientId">The client id the system authenticates with: who it is, and the one an audience is granted to.</param>
/// <param name="Kind">What kind of system it is.</param>
/// <param name="Name">What the product calls the system, so an operator can recognize it; empty when it has no name.</param>
public record SystemDeclaration(SystemClientId ClientId, SystemKind Kind, string Name);
