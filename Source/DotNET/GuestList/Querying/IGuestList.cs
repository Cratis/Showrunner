// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Querying;

/// <summary>
/// Defines what the product knows about the audiences of its tenants, users and the system. It is what an availability
/// engine reads to build the audiences of a request, and it answers from what the product recorded: no call is made to the
/// operator, and the last known state stays when the connection to the operator is down.
/// </summary>
public interface IGuestList
{
    /// <summary>
    /// Gets the audiences granted to the system, which apply to every tenant of the product.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The audiences, empty when none is granted.</returns>
    Task<IReadOnlySet<AudienceName>> ForSystem(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the audiences granted to a tenant as a whole.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The audiences, empty when none is granted.</returns>
    Task<IReadOnlySet<AudienceName>> ForTenant(TenantId tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the audiences granted to one user of a tenant.
    /// </summary>
    /// <param name="tenant">The tenant of the user.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The audiences, empty when none is granted.</returns>
    Task<IReadOnlySet<AudienceName>> ForUser(TenantId tenant, UserId user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the audiences that apply to a request: the tenant's, the user's and the system's, added together. Withdrawing a
    /// grant removed it from its own level only, so an audience granted at two levels stays while either holds it.
    /// </summary>
    /// <param name="tenant">The tenant of the request.</param>
    /// <param name="user">The user of the request; <see cref="UserId.NotSet"/> for a request that is not a person.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The audiences, empty when none applies.</returns>
    Task<IReadOnlySet<AudienceName>> For(TenantId tenant, UserId user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets every grant that applies to a tenant: the tenant's own, those of its users, and the system's.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The grants.</returns>
    Task<IReadOnlyList<GrantedAudience>> Granted(TenantId tenant, CancellationToken cancellationToken = default);
}
