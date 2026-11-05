// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning;

/// <summary>
/// The answer of a product to being asked to create a tenant.
/// </summary>
/// <param name="IsProvisioned">Whether the tenant exists and is ready to use.</param>
/// <param name="Reason">Why the product refused, when it did.</param>
public record TenantProvisioning(bool IsProvisioned, NotProvisionedReason Reason)
{
    /// <summary>
    /// The tenant exists and is ready to use.
    /// </summary>
    public static readonly TenantProvisioning Provisioned = new(true, NotProvisionedReason.NotSet);

    /// <summary>
    /// The product refuses to create the tenant, for a reason that trying again will not change.
    /// </summary>
    /// <param name="reason">Why the product refuses, in words the operator can show to a person.</param>
    /// <returns>The <see cref="TenantProvisioning"/> to answer with.</returns>
    public static TenantProvisioning Refused(NotProvisionedReason reason) => new(false, reason);
}
