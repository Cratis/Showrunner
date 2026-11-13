// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Availability.Declaration;

/// <summary>
/// Reports the product's declaration when the host starts, in the background. It never holds the host back: when Chronicle
/// cannot be reached it tries again every <see cref="GuestListOptions.RetryInterval"/> until it has reported or the
/// declaration is known to be the operator's already.
/// </summary>
/// <param name="client">The <see cref="IChronicleClient"/>.</param>
/// <param name="chronicle">The <see cref="ChronicleClientOptions"/>, which name the product's event store.</param>
/// <param name="reporter">The <see cref="AvailabilityReporter"/>.</param>
/// <param name="options">The <see cref="GuestListOptions"/>.</param>
/// <param name="time">The <see cref="TimeProvider"/>.</param>
/// <param name="logger">The <see cref="ILogger{AvailabilityReportingService}"/>.</param>
public class AvailabilityReportingService(
    IChronicleClient client,
    IOptions<ChronicleClientOptions> chronicle,
    AvailabilityReporter reporter,
    IOptions<GuestListOptions> options,
    TimeProvider time,
    ILogger<AvailabilityReportingService> logger) : BackgroundService
{
    /// <summary>
    /// Reports once.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>What reporting came to.</returns>
    public async Task<AvailabilityReport> RunOnce(CancellationToken cancellationToken = default)
    {
        var eventStore = await client.GetEventStore(chronicle.Value.EventStore, EventStoreNamespaceName.Default).WaitAsync(cancellationToken);
        return await reporter.Report(eventStore);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
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
