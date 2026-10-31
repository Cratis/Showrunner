// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;

namespace Cratis.Showrunner.Marquee.given;

/// <summary>
/// The people and tenants the specifications run as.
/// </summary>
public static class Viewers
{
    /// <summary>
    /// The subject of the person the specifications run as.
    /// </summary>
    public static readonly UserSubject Me = new("person-1");

    /// <summary>
    /// The subject of somebody else in the same tenant.
    /// </summary>
    public static readonly UserSubject SomebodyElse = new("person-2");

    /// <summary>
    /// Gets a signed-in principal.
    /// </summary>
    /// <param name="subject">The subject of the person.</param>
    /// <returns>The principal.</returns>
    public static ClaimsPrincipal SignedInAs(UserSubject subject) => new(new ClaimsIdentity([new Claim("sub", subject.Value)], "Specifications"));

    /// <summary>
    /// Gets a principal nobody is signed in as.
    /// </summary>
    public static ClaimsPrincipal Anonymous => new(new ClaimsIdentity());
}

/// <summary>
/// An <see cref="ICurrentPrincipalAccessor"/> that gives a fixed principal.
/// </summary>
/// <param name="principal">The principal.</param>
public class FixedPrincipal(ClaimsPrincipal principal) : ICurrentPrincipalAccessor
{
    /// <inheritdoc/>
    public ClaimsPrincipal? Current => principal;
}

/// <summary>
/// An <see cref="ICurrentTenant"/> for a fixed tenant.
/// </summary>
/// <param name="id">The tenant.</param>
public class FixedTenant(TenantId id) : ICurrentTenant
{
    /// <inheritdoc/>
    public TenantId Id => id;
}

/// <summary>
/// An <see cref="IAudienceEntitlements"/> that knows which tenants hold a module and which are on a plan.
/// </summary>
/// <param name="holders">The tenants that hold the module <c>reporting</c>.</param>
/// <param name="onPlan">The tenants that are on the plan <c>team</c>.</param>
public class FixedEntitlements(IEnumerable<TenantId> holders, IEnumerable<TenantId> onPlan) : IAudienceEntitlements
{
    /// <inheritdoc/>
    public ValueTask<bool> HoldsModule(TenantId tenantId, ModuleKey module) =>
        ValueTask.FromResult(module == "reporting" && holders.Contains(tenantId));

    /// <inheritdoc/>
    public ValueTask<bool> IsOnPlan(TenantId tenantId, PlanId plan) =>
        ValueTask.FromResult(plan == "team" && onPlan.Contains(tenantId));
}
#endif
