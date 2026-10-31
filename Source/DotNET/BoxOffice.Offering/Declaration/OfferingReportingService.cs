// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// Reports the product's offering when the product starts, and keeps trying until it succeeds.
/// </summary>
/// <param name="client">The Chronicle client.</param>
/// <param name="clientOptions">The Chronicle client options, which name the product's event store.</param>
/// <param name="reporter">The <see cref="OfferingReporter"/>.</param>
/// <param name="options">The <see cref="OfferingOptions"/>.</param>
/// <param name="time">The time provider.</param>
/// <param name="logger">The logger.</param>
public class OfferingReportingService(
    IChronicleClient client,
    IOptions<ChronicleClientOptions> clientOptions,
    OfferingReporter reporter,
    IOptions<OfferingOptions> options,
    TimeProvider time,
    ILogger<OfferingReportingService> logger) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var eventStore = await client.GetEventStore(clientOptions.Value.EventStore, EventStoreNamespaceName.Default).WaitAsync(stoppingToken);
                await reporter.Report(eventStore);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.ReportingFailed(exception);
            }

            try
            {
                await Task.Delay(options.Value.RetryInterval, time, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
