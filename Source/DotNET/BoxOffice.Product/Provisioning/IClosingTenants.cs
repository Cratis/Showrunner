// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning;

/// <summary>
/// Tears down a tenant the operator is closing, because its organization is being deleted. It is optional: a product
/// that implements it, and registers it with its services, is told.
/// </summary>
public interface IClosingTenants
{
    /// <summary>
    /// Tears the tenant down. The operator is told nothing back, and the call can be repeated, so tearing down a tenant
    /// that is gone must change nothing.
    /// </summary>
    /// <param name="tenant">The tenant that is closing.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the work.</param>
    /// <returns>A <see cref="Task"/> for continuation.</returns>
    Task Closing(TenantId tenant, CancellationToken cancellationToken = default);
}
