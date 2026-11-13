// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Audiences;

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// An audience the product's model declares.
/// </summary>
/// <param name="Name">The audience.</param>
/// <param name="Description">What the audience is for, as the model says; empty when it says nothing.</param>
/// <param name="Source">Where the model declares it, such as <c>availability.screenplay:3</c>; empty when unknown.</param>
public record AudienceDeclaration(AudienceName Name, string Description, string Source);
