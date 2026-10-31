// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Audiences;

/// <summary>
/// The <see cref="IAudienceEntitlements"/> to use when a host has no entitlement source: no tenant holds any module and
/// no tenant is on any plan.
/// </summary>
public sealed class NoAudienceEntitlements : IAudienceEntitlements
{
    /// <summary>
    /// Gets the shared instance.
    /// </summary>
    public static readonly NoAudienceEntitlements Instance = new();

    /// <inheritdoc/>
    public ValueTask<bool> HoldsModule(TenantId tenantId, ModuleKey module) => ValueTask.FromResult(false);

    /// <inheritdoc/>
    public ValueTask<bool> IsOnPlan(TenantId tenantId, PlanId plan) => ValueTask.FromResult(false);
}
