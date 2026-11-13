// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Audiences;

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// Published by the product, once per change, to say what its model declares: the audiences, the availability blocks,
/// the gates and the values and personas they refer to. It is what the product's own engine evaluates, reported as
/// data; it says nothing about who holds what, and is not repeated while it is unchanged. The operator uses it to
/// show what can be granted, and to know which audiences the product declares.
/// </summary>
/// <param name="Product">The product that declares.</param>
/// <param name="Environment">The environment the product reports from; not set when the product does not name it.</param>
/// <param name="Version">The version of the product build.</param>
/// <param name="Origin">Where the model comes from, such as <c>Studio/availability.screenplay</c>.</param>
/// <param name="EveryDeclaredAudienceGranted">Whether this environment grants every declared audience to everyone.</param>
/// <param name="Audiences">The audiences declared.</param>
/// <param name="Availabilities">The availability blocks declared.</param>
/// <param name="Gates">The gates declared.</param>
/// <param name="ContextValues">The context values the conditions read.</param>
/// <param name="Personas">The personas declared.</param>
[EventType]
[Public]
public record AvailabilityDeclared(
    Product Product,
    EnvironmentName Environment,
    ModelVersion Version,
    string Origin,
    bool EveryDeclaredAudienceGranted,
    IEnumerable<AudienceDeclaration> Audiences,
    IEnumerable<AvailabilityBlockDeclaration> Availabilities,
    IEnumerable<GateDeclaration> Gates,
    IEnumerable<ContextValueDeclaration> ContextValues,
    IEnumerable<PersonaDeclaration> Personas);
