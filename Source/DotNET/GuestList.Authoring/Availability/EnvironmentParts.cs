// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring.Availability;

/// <summary>
/// The parts a product last reported for one environment.
/// </summary>
/// <param name="Environment">The environment the product reported from.</param>
/// <param name="DeclaredAt">When the product reported them.</param>
/// <param name="Parts">Every part, in the order of their paths.</param>
public record EnvironmentParts(EnvironmentName Environment, DateTimeOffset DeclaredAt, IEnumerable<PartDeclaration> Parts);
