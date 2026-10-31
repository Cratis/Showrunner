// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Audiences;

/// <summary>
/// Who an announcement reaches. <see cref="Kind"/> says which of the other values applies; the others are empty
/// (<see cref="ModuleKey.NotSet"/>, <see cref="PlanId.NotSet"/>, no tenants) when they do not, so nothing in the payload is
/// null. Build one with <see cref="Everyone"/>, <see cref="Tenants"/>, <see cref="HoldersOfModule"/> or
/// <see cref="OnPlan"/>. Whether a tenant is in it is answered by <see cref="Includes"/>.
/// </summary>
/// <param name="Kind">Which kind of audience this is.</param>
/// <param name="TenantIds">The tenants named, for <see cref="AudienceKind.Tenants"/>.</param>
/// <param name="Module">The module held, for <see cref="AudienceKind.HoldersOfModule"/>.</param>
/// <param name="Plan">The plan, for <see cref="AudienceKind.OnPlan"/>.</param>
public record Audience(AudienceKind Kind, IEnumerable<TenantId> TenantIds, ModuleKey Module, PlanId Plan)
{
    /// <summary>
    /// Gets the audience of everyone using the product.
    /// </summary>
    /// <returns>The audience.</returns>
    public static Audience Everyone() => new(AudienceKind.Everyone, [], ModuleKey.NotSet, PlanId.NotSet);

    /// <summary>
    /// Gets the audience of the named tenants.
    /// </summary>
    /// <param name="tenantIds">The tenants.</param>
    /// <returns>The audience.</returns>
    public static Audience Tenants(params IEnumerable<TenantId> tenantIds) => new(AudienceKind.Tenants, [.. tenantIds], ModuleKey.NotSet, PlanId.NotSet);

    /// <summary>
    /// Gets the audience of the tenants that hold a module.
    /// </summary>
    /// <param name="module">The module.</param>
    /// <returns>The audience.</returns>
    public static Audience HoldersOfModule(ModuleKey module) => new(AudienceKind.HoldersOfModule, [], module, PlanId.NotSet);

    /// <summary>
    /// Gets the audience of the tenants on a plan.
    /// </summary>
    /// <param name="plan">The plan.</param>
    /// <returns>The audience.</returns>
    public static Audience OnPlan(PlanId plan) => new(AudienceKind.OnPlan, [], ModuleKey.NotSet, plan);

    /// <summary>
    /// Gets whether a tenant is in this audience.
    /// </summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="entitlements">What answers module and plan membership; <see cref="NoAudienceEntitlements"/> when the host has none.</param>
    /// <returns><see langword="true"/> when the announcement reaches the tenant; otherwise <see langword="false"/>.</returns>
    public ValueTask<bool> Includes(TenantId tenantId, IAudienceEntitlements entitlements) => Kind switch
    {
        AudienceKind.Everyone => ValueTask.FromResult(true),
        AudienceKind.Tenants => ValueTask.FromResult(TenantIds.Contains(tenantId)),
        AudienceKind.HoldersOfModule => entitlements.HoldsModule(tenantId, Module),
        AudienceKind.OnPlan => entitlements.IsOnPlan(tenantId, Plan),
        _ => ValueTask.FromResult(false)
    };
}
