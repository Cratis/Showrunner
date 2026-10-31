// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Tenancy;

namespace Cratis.Showrunner.Marquee.People;

/// <summary>
/// Defines the tenant of the current request.
/// </summary>
public interface ICurrentTenant
{
    /// <summary>
    /// Gets the tenant, as an audience names it: the tenant id, which is also the name of the tenant's namespace.
    /// </summary>
    Contracts.TenantId Id { get; }
}

/// <summary>
/// Represents an implementation of <see cref="ICurrentTenant"/> that reads Arc's tenant of the request.
/// </summary>
/// <param name="tenants">The <see cref="ITenantIdAccessor"/> giving the tenant of the request.</param>
public class CurrentTenant(ITenantIdAccessor tenants) : ICurrentTenant
{
    /// <inheritdoc/>
    public Contracts.TenantId Id => new(tenants.Current.Value);
}
