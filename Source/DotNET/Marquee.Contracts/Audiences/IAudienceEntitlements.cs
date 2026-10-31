// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Audiences;

/// <summary>
/// Answers the two questions an <see cref="Audience"/> cannot answer from its own values: whether a tenant holds a
/// module, and whether a tenant is on a plan. Marquee depends on this abstraction and on nothing that sells or meters a
/// product, so a host with an entitlement source (Box Office, or its own) plugs it in, and a host without one still
/// gets <see cref="AudienceKind.Everyone"/> and <see cref="AudienceKind.Tenants"/> audiences. With
/// <see cref="NoAudienceEntitlements"/>, module and plan audiences reach nobody.
/// </summary>
public interface IAudienceEntitlements
{
    /// <summary>
    /// Gets whether a tenant holds a module.
    /// </summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="module">The module.</param>
    /// <returns><see langword="true"/> when the tenant holds the module; otherwise <see langword="false"/>.</returns>
    ValueTask<bool> HoldsModule(TenantId tenantId, ModuleKey module);

    /// <summary>
    /// Gets whether a tenant is on a plan.
    /// </summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="plan">The plan.</param>
    /// <returns><see langword="true"/> when the tenant is on the plan; otherwise <see langword="false"/>.</returns>
    ValueTask<bool> IsOnPlan(TenantId tenantId, PlanId plan);
}
