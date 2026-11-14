// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Availability.Systems;

/// <summary>
/// Defines which systems call the product, such as integrations, MCP clients and API callers, so the operator can offer them
/// when an audience is granted to one system. The product knows its clients, for instance from the registrations of its
/// identity provider; GuestList only reports what it is told.
/// </summary>
public interface IKnownSystems
{
    /// <summary>
    /// Gets the systems the product knows now.
    /// </summary>
    IReadOnlyCollection<SystemDeclaration> Current { get; }
}
