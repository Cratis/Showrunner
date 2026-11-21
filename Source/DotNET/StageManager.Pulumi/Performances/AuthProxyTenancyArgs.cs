// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// How AuthProxy resolves the tenant of a request: a fixed tenant, or an endpoint that lists the tenants of the signed-in person.
/// </summary>
public sealed class AuthProxyTenancyArgs
{
    /// <summary>
    /// Gets the fixed tenant every request resolves to. Give this or <see cref="TenantsEndpoint"/>.
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets the URL that lists the tenants the signed-in person belongs to. Give this or <see cref="TenantId"/>.
    /// </summary>
    public string? TenantsEndpoint { get; init; }

    /// <summary>
    /// Builds the spec of the tenancy.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("tenantId", TenantId)
            .Put("tenantsEndpoint", TenantsEndpoint);
}
