// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Audiences;

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// One way a gate or a block opens: every audience listed is held and every condition holds. It opens when any of its
/// alternatives does, so the operator needs no expression evaluator.
/// </summary>
/// <param name="Audiences">The audiences that must all be held.</param>
/// <param name="Conditions">The conditions that must all hold, as the model wrote them. They are shown, never evaluated by the operator.</param>
public record AvailabilityAlternative(IEnumerable<AudienceName> Audiences, IEnumerable<string> Conditions);
