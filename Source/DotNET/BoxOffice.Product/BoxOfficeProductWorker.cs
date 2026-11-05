// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Product.Provisioning;
using Cratis.Showrunner.BoxOffice.Tenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product;

/// <summary>
/// Starts listening for the tenants the operator registers and closes, once the product's event store is reachable.
/// </summary>
/// <param name="tenants">The <see cref="TenantEventStores"/> that gives the product's event store.</param>
/// <param name="subscriptions">The <see cref="ProvisioningSubscriptions"/> to register.</param>
/// <param name="services">The <see cref="IServiceProviderIsService"/> that says whether the product provisions tenants.</param>
/// <param name="options">The <see cref="BoxOfficeOptions"/>, which say how often to try.</param>
/// <param name="time">The <see cref="TimeProvider"/> the waiting is timed by.</param>
/// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
public class BoxOfficeProductWorker(
    TenantEventStores tenants,
    ProvisioningSubscriptions subscriptions,
    IServiceProviderIsService services,
    IOptions<BoxOfficeOptions> options,
    TimeProvider time,
    ILogger<BoxOfficeProductWorker> logger) : BackgroundService
{
    /// <summary>
    /// Registers the listening once. A product that does not implement <see cref="IProvisionTenants"/> creates no tenants,
    /// so nothing is registered.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the work.</param>
    /// <returns>A <see cref="Task"/> for continuation.</returns>
    public async Task RunOnce(CancellationToken cancellationToken = default)
    {
        if (!services.IsService(typeof(IProvisionTenants)))
        {
            logger.NotProvisioning();
            return;
        }

        // The operators' events arrive in the inbox of the default namespace, which is the first store.
        var stores = await tenants.All(cancellationToken);
        await subscriptions.Register(stores[0]);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnce(stoppingToken);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.RoundFailed(exception);
            }

            try
            {
                await Task.Delay(options.Value.PollingInterval, time, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
