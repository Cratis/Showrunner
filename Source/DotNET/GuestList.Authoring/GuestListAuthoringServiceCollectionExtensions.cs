// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Authoring;
using Cratis.Showrunner.GuestList.Authoring.Tenancy;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding GuestList's authoring services to a service collection.
/// </summary>
public static class GuestListAuthoringServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services GuestList's authoring commands and queries use. Authorization is the host's: register a policy
    /// named <see cref="GuestListAuthoring.Policy"/>.
    /// </summary>
    /// <remarks>
    /// Register an <see cref="IKnownTenants"/> that knows the tenants of the products before or after this call and it is
    /// used; without one, any named tenant is accepted and the product refuses a tenant it does not have when it receives
    /// the decision.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="configure">Configures the authoring side; optional.</param>
    /// <returns>The service collection for continuation.</returns>
    public static IServiceCollection AddGuestListAuthoring(this IServiceCollection services, Action<GuestListAuthoringOptions>? configure = null)
    {
        var options = services.AddOptions<GuestListAuthoringOptions>();
        if (configure is not null)
        {
            options.Configure(configure);
        }

        services.TryAddSingleton<IKnownTenants, AnyNamedTenant>();
        return services;
    }
}
