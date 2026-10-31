// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Entitlements;

/// <summary>
/// What the operator last decided about a tenant and a module.
/// </summary>
/// <param name="Module">The module.</param>
/// <param name="Access">What the tenant may do with the module.</param>
/// <param name="Until">The last day the access holds. A revoked module has none.</param>
/// <param name="DecidedAt">When the operator decided it.</param>
public record ModuleEntitlement(ModuleId Module, ModuleAccess Access, EntitlementEnd Until, DateTimeOffset DecidedAt)
{
    /// <summary>
    /// Gets what the tenant may do with the module on a day. Access ends after its last day, without any event.
    /// </summary>
    /// <param name="today">The day.</param>
    /// <returns>The access that stands, <see cref="ModuleAccess.None"/> once it has ended.</returns>
    public ModuleAccess On(DateOnly today) => Access != ModuleAccess.None && today > Until.Value ? ModuleAccess.None : Access;
}
