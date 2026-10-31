// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Contracts.Offering;

namespace Cratis.Showrunner.BoxOffice.Entitlements;

/// <summary>
/// What the tenant may do with one of the product's modules today.
/// </summary>
/// <param name="Module">The module.</param>
/// <param name="Name">What the module is called.</param>
/// <param name="Access">The access the tenant has today.</param>
/// <param name="Until">The last day of the access; nothing when the access does not end or the tenant has none.</param>
public record ModuleStanding(ModuleId Module, ModuleName Name, ModuleAccess Access, DateOnly? Until);
