// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// Measures the compute of every tenant of the product while the product runs.
/// </summary>
/// <remarks>
/// A tenant is a namespace of the product's event store. Only one instance of the product measures at a time, the one
/// that holds the <see cref="IMeteringStore"/>; the others wait and take over when its lease runs out. Creating the
/// service checks that the registered meters are declared, so a product that measures what it does not sell does not start.
/// </remarks>
public class MeteringService : BackgroundService
{
    readonly IChronicleClient _client;
    readonly IOptions<ChronicleClientOptions> _clientOptions;
    readonly IMeteringStore _store;
    readonly TenantMetering _metering;
    readonly IOptions<MeteringOptions> _options;
    readonly TimeProvider _time;
    readonly ILogger<MeteringService> _logger;
    readonly string _holder = $"{Environment.MachineName}-{Guid.NewGuid():N}";

    /// <summary>
    /// Initializes a new instance of the <see cref="MeteringService"/> class.
    /// </summary>
    /// <param name="client">The Chronicle client.</param>
    /// <param name="clientOptions">The Chronicle client options, which name the product's event store.</param>
    /// <param name="offering">The product's offering.</param>
    /// <param name="meters">The product's meters.</param>
    /// <param name="store">The <see cref="IMeteringStore"/>.</param>
    /// <param name="metering">The <see cref="TenantMetering"/>.</param>
    /// <param name="options">The <see cref="MeteringOptions"/>.</param>
    /// <param name="time">The time provider.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="InvalidMetering">Thrown when a meter is not declared in the offering.</exception>
    public MeteringService(
        IChronicleClient client,
        IOptions<ChronicleClientOptions> clientOptions,
        ProductOffering offering,
        IEnumerable<IMeter> meters,
        IMeteringStore store,
        TenantMetering metering,
        IOptions<MeteringOptions> options,
        TimeProvider time,
        ILogger<MeteringService> logger)
    {
        var problems = MeteringProblems.In(offering, meters).ToArray();
        if (problems.Length > 0)
        {
            throw new InvalidMetering(problems);
        }

        _client = client;
        _clientOptions = clientOptions;
        _store = store;
        _metering = metering;
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
                await MeasureAll(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.CycleFailed(exception);
            }

            try
            {
                await Task.Delay(_options.Value.PollInterval, _time, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    async Task MeasureAll(CancellationToken cancellationToken)
    {
        var now = _time.GetUtcNow();
        if (!await _store.TryHold(_holder, _options.Value.Lease, now))
        {
            return;
        }

        var name = _clientOptions.Value.EventStore;
        var eventStore = await _client.GetEventStore(name, EventStoreNamespaceName.Default).WaitAsync(cancellationToken);
        foreach (var @namespace in await eventStore.GetNamespaces(cancellationToken))
        {
            if (@namespace == EventStoreNamespaceName.Default)
            {
                continue;
            }

            var tenant = new TenantId(@namespace.Value);
            try
            {
                var tenantStore = await _client.GetEventStore(name, @namespace).WaitAsync(cancellationToken);
                await _metering.Measure(tenantStore, tenant, now);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // One tenant's failure must not stop the others; it is measured again on the next round.
                _logger.MeasuringFailed(tenant, exception);
            }
        }
    }
}
