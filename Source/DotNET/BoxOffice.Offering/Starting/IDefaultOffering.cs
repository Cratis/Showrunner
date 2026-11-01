// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Starting;

/// <summary>
/// Defines how a product starts a new tenant on the offer it declared as its default, without waiting for an
/// operator decision.
/// </summary>
public interface IDefaultOffering
{
    /// <summary>
    /// Starts a tenant on the default offering and tells the operator, as <c>TenantStartedOnOffer</c>. Starting a tenant
    /// that already started tells the operator nothing again.
    /// </summary>
    /// <param name="tenant">The tenant, the name of its Chronicle namespace in the product.</param>
    /// <param name="organization">The organization the tenant belongs to; the operator's facts about the tenant are recorded under it.</param>
    /// <param name="cancellationToken">Cancels the start.</param>
    /// <returns>Whether the tenant started now or had started already.</returns>
    /// <exception cref="DefaultOfferingNotDeclared">The product declared no default offering.</exception>
    /// <exception cref="DefaultOfferingNotPublished">The operator has not published an offer with the default offering's name, or has withdrawn it.</exception>
    /// <exception cref="TenantNotStarted">The event store refused the fact.</exception>
    Task<TenantStart> StartTenant(TenantId tenant, OrganizationId organization, CancellationToken cancellationToken = default);
}
