// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring.Tenancy;

/// <summary>
/// Defines what the operator side knows of the tenants of a product. A grant for a tenant is refused when the tenant is not
/// known, so a typo is caught where the operator types it and not when it fails to arrive.
/// </summary>
/// <remarks>
/// The operator side has no tenant register of its own: the host supplies one that fits where its tenants are kept, for
/// example Box Office's, by registering its implementation before <c>AddGuestListAuthoring</c>. The default,
/// <see cref="AnyNamedTenant"/>, knows no more than that a tenant is named.
/// </remarks>
public interface IKnownTenants
{
    /// <summary>
    /// Gets whether a tenant of a product exists.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="tenant">The tenant.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns><see langword="true"/> when the product has the tenant; otherwise <see langword="false"/>.</returns>
    Task<bool> Exists(Product product, TenantId tenant, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an implementation of <see cref="IKnownTenants"/> that accepts any named tenant. It is the default, for a host
/// that has no register of tenants: the product refuses a decision about a tenant it does not have when it receives it.
/// </summary>
public class AnyNamedTenant : IKnownTenants
{
    /// <inheritdoc/>
    public Task<bool> Exists(Product product, TenantId tenant, CancellationToken cancellationToken = default) =>
        Task.FromResult(!string.IsNullOrWhiteSpace(tenant.Value));
}
