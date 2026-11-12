// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using ITenantIdAccessor = Cratis.Arc.Tenancy.ITenantIdAccessor;

namespace Cratis.Showrunner.GuestList.Querying;

/// <summary>
/// An audience granted at one level, as the product holds it: the last known decision of the operator. The queries answer
/// from what the product recorded, so they keep answering when the connection to the operator is down.
/// </summary>
/// <param name="Level">Whom the audience is granted to.</param>
/// <param name="UserId">The user, for <see cref="AudienceLevel.User"/>; empty otherwise.</param>
/// <param name="Audience">The audience.</param>
[ReadModel]
public record GrantedAudience(AudienceLevel Level, UserId UserId, AudienceName Audience)
{
    /// <summary>
    /// Gets the audiences granted to the tenant of the request, as a whole. Nothing granted to one of its users or to the
    /// system is included.
    /// </summary>
    /// <param name="guestList">The audiences the product holds.</param>
    /// <param name="tenants">The tenant of the request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The audiences granted to the tenant.</returns>
    [Authorize]
    public static async Task<IEnumerable<GrantedAudience>> ForTenant(IGuestList guestList, ITenantIdAccessor tenants, CancellationToken cancellationToken) =>
        [.. (await guestList.ForTenant(new TenantId(tenants.Current.Value), cancellationToken)).Select(audience => new GrantedAudience(AudienceLevel.Tenant, UserId.NotSet, audience))];

    /// <summary>
    /// Gets the audiences granted to one user of the tenant of the request. Nothing granted to the tenant or to the system
    /// is included.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="guestList">The audiences the product holds.</param>
    /// <param name="tenants">The tenant of the request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The audiences granted to the user.</returns>
    [Authorize]
    public static async Task<IEnumerable<GrantedAudience>> ForUser(UserId user, IGuestList guestList, ITenantIdAccessor tenants, CancellationToken cancellationToken) =>
        [.. (await guestList.ForUser(new TenantId(tenants.Current.Value), user, cancellationToken)).Select(audience => new GrantedAudience(AudienceLevel.User, user, audience))];

    /// <summary>
    /// Gets the audiences granted to the system, which apply to every tenant of the product.
    /// </summary>
    /// <param name="guestList">The audiences the product holds.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The audiences granted to the system.</returns>
    [Authorize]
    public static async Task<IEnumerable<GrantedAudience>> ForSystem(IGuestList guestList, CancellationToken cancellationToken) =>
        [.. (await guestList.ForSystem(cancellationToken)).Select(audience => new GrantedAudience(AudienceLevel.System, UserId.NotSet, audience))];
}
