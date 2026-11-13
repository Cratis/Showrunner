// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring.Availability;

/// <summary>
/// What a product last declared about its availability in one environment.
/// </summary>
/// <param name="Environment">The environment the product reported from.</param>
/// <param name="Version">The version of the product build.</param>
/// <param name="DeclaredAt">When the declaration was reported.</param>
/// <param name="Origin">Where the model comes from.</param>
/// <param name="EveryDeclaredAudienceGranted">Whether the environment grants every declared audience to everyone, whatever the operator decided.</param>
/// <param name="Audiences">The audiences declared, in the order of their names.</param>
/// <param name="Availabilities">The availability blocks declared, in the order of their names.</param>
/// <param name="Gates">The gates declared, in the order of their targets.</param>
/// <param name="ContextValues">The context values the conditions read, in the order of their names.</param>
/// <param name="Personas">The personas declared, in the order of their names.</param>
public record EnvironmentAvailability(
    EnvironmentName Environment,
    ModelVersion Version,
    DateTimeOffset DeclaredAt,
    string Origin,
    bool EveryDeclaredAudienceGranted,
    IEnumerable<AudienceDeclaration> Audiences,
    IEnumerable<AvailabilityBlockDeclaration> Availabilities,
    IEnumerable<GateDeclaration> Gates,
    IEnumerable<ContextValueDeclaration> ContextValues,
    IEnumerable<PersonaDeclaration> Personas);
