// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// A module of the product, as the product declares it.
/// </summary>
/// <param name="Id">The module's key.</param>
/// <param name="Name">The module's name as a person reads it.</param>
/// <param name="Requires">The modules a tenant must also have to use this one.</param>
/// <param name="Namespaces">The code namespaces the module owns. The longest matching namespace decides which module a type belongs to.</param>
/// <param name="DefaultTrialDays">The length in days of a trial of the module unless the operator decides otherwise; zero when it has no trial.</param>
/// <param name="Availability">How the module can be had.</param>
public record ModuleDefinition(
    ModuleId Id,
    ModuleName Name,
    IEnumerable<ModuleId> Requires,
    IEnumerable<string> Namespaces,
    int DefaultTrialDays,
    ModuleAvailability Availability);
