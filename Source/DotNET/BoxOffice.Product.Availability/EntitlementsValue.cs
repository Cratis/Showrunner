// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Availability;

/// <summary>
/// What Box Office entitles the current tenant to, as the value <c>$context.entitlements</c> an availability gate reads.
/// </summary>
/// <remarks>
/// Property names are what a gate writes, in any casing: <c>$context.entitlements.active</c>,
/// <c>$context.entitlements.readOnlyReason</c>, <c>$context.entitlements.seats</c> and
/// <c>$context.entitlements.modules.&lt;module&gt;.level</c>.
/// </remarks>
/// <param name="Active">Whether the tenant can change things: it is not suspended and its trial has not closed without seats.</param>
/// <param name="ReadOnlyReason">Why the tenant can not change things: <c>None</c>, <c>TenantSuspended</c> or <c>TrialClosed</c>.</param>
/// <param name="Seats">How many seats the tenant may use.</param>
/// <param name="Modules">Every module the product declares, by module id, and what the tenant may do with it.</param>
public record EntitlementsValue(
    bool Active,
    string ReadOnlyReason,
    int Seats,
    IReadOnlyDictionary<string, ModuleEntitlementValue> Modules);
