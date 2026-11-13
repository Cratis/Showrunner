// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList;
using Cratis.Showrunner.GuestList.Querying;
using Cratis.Showrunner.GuestList.Receiving;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding GuestList's services to a service collection.
/// </summary>
public static class GuestListServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services GuestList's queries use, and validates the options when the host starts.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="configure">Configures GuestList, at least <see cref="GuestListOptions.Product"/>.</param>
    /// <returns>The service collection for continuation.</returns>
    public static IServiceCollection AddGuestList(this IServiceCollection services, Action<GuestListOptions> configure)
    {
        services
            .AddOptions<GuestListOptions>()
            .Configure(configure)
            .Validate(options => !string.IsNullOrWhiteSpace(options.Product.Value), "GuestList needs the product it records audiences for: set GuestListOptions.Product in AddGuestList.")
            .Validate(options => options.OperatorStores.Any(), "GuestList needs at least one operator store: set GuestListOptions.OperatorStores in AddGuestList.")
            .Validate(options => options.PollingInterval > TimeSpan.Zero, "GuestList polls at an interval: set GuestListOptions.PollingInterval to more than zero.")
            .Validate(options => options.RetryInterval > TimeSpan.Zero, "GuestList retries at an interval: set GuestListOptions.RetryInterval to more than zero.")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<GuestListInbox>();
        services.TryAddSingleton<GuestListSubscriptions>();
        services.TryAddSingleton<AudienceReceipts>();
        services.TryAddSingleton<IGuestList, GuestLists>();
        services.AddHostedService<GuestListWorker>();
        return services;
    }
}
