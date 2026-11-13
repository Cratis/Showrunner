// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Audiences;

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// A persona of the model, with the audiences it holds.
/// </summary>
/// <param name="Name">The name of the persona.</param>
/// <param name="Audiences">The audiences it holds.</param>
public record PersonaDeclaration(string Name, IEnumerable<AudienceName> Audiences);
