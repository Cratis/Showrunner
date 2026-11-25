// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Querying;

/// <summary>
/// Defines what the product knows about the parts an operator suspended for its tenants, users and the system. It answers
/// from what the product recorded: no call is made to the operator, and the last known state stays when the connection to
/// the operator is down.
/// </summary>
/// <remarks>
/// Each answer holds the newest decision per part at that level, resumes included, so the caller can tell a part that was
/// resumed from one that was never decided.
/// </remarks>
public interface IPartSuspensions
{
    /// <summary>
    /// Gets the decisions about parts for the system.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The newest decision per part.</returns>
    Task<IReadOnlyList<PartDecision>> ForSystem(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the decisions about parts for a tenant.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The newest decision per part.</returns>
    Task<IReadOnlyList<PartDecision>> ForTenant(TenantId tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the decisions about parts for a user of a tenant.
    /// </summary>
    /// <param name="tenant">The tenant of the user.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The newest decision per part.</returns>
    Task<IReadOnlyList<PartDecision>> ForUser(TenantId tenant, UserId user, CancellationToken cancellationToken = default);
}
