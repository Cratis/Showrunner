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
public class OfferingReportingService : BackgroundService
{
    readonly IChronicleClient _client;
    readonly BoxOfficeEventStore _eventStore;
    readonly OfferingReporter _reporter;
    readonly IOptions<OfferingOptions> _options;
    readonly TimeProvider _time;
    readonly ILogger<OfferingReportingService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OfferingReportingService"/> class, resolving the event store from the
    /// Chronicle client options only.
    /// </summary>
    /// <param name="client">The Chronicle client.</param>
    /// <param name="clientOptions">The Chronicle client options, which name the product's event store.</param>
    /// <param name="reporter">The <see cref="OfferingReporter"/>.</param>
    /// <param name="options">The <see cref="OfferingOptions"/>.</param>
    /// <param name="time">The time provider.</param>
    /// <param name="logger">The logger.</param>
    public OfferingReportingService(
        IChronicleClient client,
        IOptions<ChronicleClientOptions> clientOptions,
        OfferingReporter reporter,
        IOptions<OfferingOptions> options,
        TimeProvider time,
        ILogger<OfferingReportingService> logger)
        : this(client, clientOptions, reporter, options, time, logger, BoxOfficeEventStore.ForClientOptions(clientOptions, options))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OfferingReportingService"/> class.
    /// </summary>
    /// <param name="client">The Chronicle client.</param>
    /// <param name="clientOptions">The Chronicle client options. The event store is named by <paramref name="eventStore"/>; the parameter keeps this constructor a superset of the earlier one, so dependency injection chooses it.</param>
    /// <param name="reporter">The <see cref="OfferingReporter"/>.</param>
    /// <param name="options">The <see cref="OfferingOptions"/>.</param>
    /// <param name="time">The time provider.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="eventStore">The <see cref="BoxOfficeEventStore"/> naming the product's event store.</param>
    public OfferingReportingService(
        IChronicleClient client,
        IOptions<ChronicleClientOptions> clientOptions,
        OfferingReporter reporter,
        IOptions<OfferingOptions> options,
        TimeProvider time,
        ILogger<OfferingReportingService> logger,
        BoxOfficeEventStore eventStore)
    {
        _ = clientOptions;
        _client = client;
        _eventStore = eventStore;
        _reporter = reporter;
        _options = options;
        _time = time;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var eventStore = await _client.GetEventStore(_eventStore.Name, EventStoreNamespaceName.Default).WaitAsync(stoppingToken);
                await _reporter.Report(eventStore);
                await _reporter.ReportDefaultOffering(eventStore);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.ReportingFailed(exception);
            }

            try
            {
                await Task.Delay(_options.Value.RetryInterval, _time, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
