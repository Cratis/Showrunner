// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring.Availability;

/// <summary>
/// The systems a product last said call it from one environment.
/// </summary>
/// <param name="Environment">The environment the product reported from.</param>
/// <param name="DeclaredAt">When the list was reported.</param>
/// <param name="Systems">The systems, in the order of their client ids.</param>
public record EnvironmentSystems(EnvironmentName Environment, DateTimeOffset DeclaredAt, IEnumerable<SystemDeclaration> Systems);
