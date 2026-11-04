// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.BoxOffice.Product.Tenancy;

/// <summary>
/// Finds the event store of a tenant: the product's Chronicle namespace that the tenant's <see cref="TenantId"/> names.
/// </summary>
public interface ITenantStores
{
    /// <summary>
    /// Gets the event store of a tenant.
    /// </summary>
    /// <param name="tenant">The tenant to find.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the lookup.</param>
    /// <returns>The <see cref="IEventStore"/> of the tenant, or <see langword="null"/> when the product has no such tenant.</returns>
    Task<IEventStore?> For(TenantId tenant, CancellationToken cancellationToken = default);
}
