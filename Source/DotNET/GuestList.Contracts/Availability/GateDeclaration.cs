// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// A gate of the model: something that is available only for some audiences.
/// </summary>
/// <param name="Target">What the gate guards, as the model names it, such as a module, a feature or a slice.</param>
/// <param name="Alternatives">The ways the gate opens. An alternative without audiences and conditions always opens.</param>
public record GateDeclaration(string Target, IEnumerable<AvailabilityAlternative> Alternatives);
