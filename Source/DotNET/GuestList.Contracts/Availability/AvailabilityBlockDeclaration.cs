// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// A named availability block of the model: who it is for, already expanded into its alternatives.
/// </summary>
/// <param name="Name">The name of the block.</param>
/// <param name="Description">What the block is for; empty when the model says nothing.</param>
/// <param name="Source">Where the model declares it; empty when unknown.</param>
/// <param name="Alternatives">The ways the block opens.</param>
public record AvailabilityBlockDeclaration(string Name, string Description, string Source, IEnumerable<AvailabilityAlternative> Alternatives);
