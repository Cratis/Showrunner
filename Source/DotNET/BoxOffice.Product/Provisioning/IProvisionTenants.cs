// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning;

/// <summary>
/// Creates the tenants an operator registers. A product implements it, and registers it with its services, to take part
/// in provisioning: Box Office hears the operator's registration, calls it and reports the answer.
/// </summary>
/// <remarks>
/// The registration can be delivered more than once, so creating a tenant that already exists must change nothing and
/// answer <see cref="TenantProvisioning.Provisioned"/>. Throw for a failure that is worth another attempt: the
/// registration is retried. Answer <see cref="TenantProvisioning.Refused"/> only for a refusal that retrying will not change.
/// </remarks>
public interface IProvisionTenants
{
    /// <summary>
    /// Creates the tenant: its data, and the first role of the person who registered it.
    /// </summary>
    /// <param name="tenant">The <see cref="TenantToProvision"/> to create.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the work.</param>
    /// <returns>Whether the tenant is ready.</returns>
    Task<TenantProvisioning> Provision(TenantToProvision tenant, CancellationToken cancellationToken = default);
}
