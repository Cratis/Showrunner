// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// How AuthProxy resolves the tenant of a request. Give a fixed tenant or an endpoint that lists the tenants of the signed-in person, not both. Omit both to leave tenancy to AuthProxy's default.
/// </summary>
public class AuthProxyTenancySpec
{
    /// <summary>
    /// Gets or sets the fixed tenant every request resolves to.
    /// </summary>
    [Description("A fixed tenant every request resolves to, for a service that is not multi-tenant. Give this or tenantsEndpoint.")]
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the endpoint that lists the tenants the signed-in person belongs to.
    /// </summary>
    [Description("The URL that lists the tenants the signed-in person belongs to: a person with one goes straight in, a person with several chooses, a person with none is refused. Give this or tenantId.")]
    public string? TenantsEndpoint { get; set; }
}
