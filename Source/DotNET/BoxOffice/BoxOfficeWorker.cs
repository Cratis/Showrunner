// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Receiving;
using Cratis.Showrunner.BoxOffice.Tenancy;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice;

/// <summary>
/// Keeps Box Office serving every tenant: the inbox of each operator is observed in every namespace, including those that
/// appear after the application started.
/// </summary>
/// <param name="tenants">The <see cref="TenantEventStores"/>.</param>
/// <param name="operators">The <see cref="OperatorSubscriptions"/>.</param>
/// <param name="options">The <see cref="BoxOfficeOptions"/>.</param>
/// <param name="time">The <see cref="TimeProvider"/> the rounds are timed by.</param>
/// <param name="logger">The <see cref="ILogger{T}"/>.</param>
public class BoxOfficeWorker(
    TenantEventStores tenants,
    OperatorSubscriptions operators,
    IOptions<BoxOfficeOptions> options,
    TimeProvider time,
    ILogger<BoxOfficeWorker> logger) : BackgroundService
{
    /// <summary>
    /// Serves every namespace that exists now: observes the operators' inboxes in each and subscribes to their outboxes.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>An awaitable task.</returns>
    public async Task RunOnce(CancellationToken cancellationToken = default)
    {
        var stores = await tenants.All(cancellationToken);
        foreach (var store in stores)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await operators.Observe(store);
        }

        await operators.Subscribe(stores[0]);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnce(stoppingToken);
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
