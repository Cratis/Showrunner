// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// Measures one tenant's compute: adds what the product's meters read from its events to the day's buckets, reports
/// a day as closed once, and reports a correction when usage arrives for a day that is already closed.
/// </summary>
/// <remarks>
/// <para>
/// Every step can be repeated after a crash without a second report. Events are added once per bucket, a closed day
/// and a correction are appended with an idempotency key made from the tenant, the day and, for a correction, the
/// latest late event, and the store only marks them reported after the event store has them.
/// </para>
/// <para>
/// A correction carries the corrected totals of the day, not a difference, as the contract says.
/// </para>
/// </remarks>
/// <param name="store">The <see cref="IMeteringStore"/>.</param>
/// <param name="meters">The product's meters.</param>
/// <param name="options">The <see cref="MeteringOptions"/>.</param>
/// <param name="metrics">The metrics.</param>
/// <param name="logger">The logger.</param>
public class TenantMetering(
    IMeteringStore store,
    IEnumerable<IMeter> meters,
    IOptions<MeteringOptions> options,
    IMeter<TenantMetering> metrics,
    ILogger<TenantMetering> logger)
{
    readonly IMeter[] _meters = [.. meters];

    /// <summary>
    /// Measures a tenant: adds new usage, closes the days that are due and reports corrections.
    /// </summary>
    /// <param name="tenantStore">The event store in the tenant's namespace.</param>
    /// <param name="tenant">The tenant, which is the name of the namespace.</param>
    /// <param name="now">The current time.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="ComputeNotReported">Thrown when the event store refuses a compute event.</exception>
    public async Task Measure(IEventStore tenantStore, TenantId tenant, DateTimeOffset now)
    {
        await AddNewUsage(tenantStore, tenant);
        await CloseDueDays(tenantStore, tenant, now);
        await ReportCorrections(tenantStore, tenant);
    }

    async Task AddNewUsage(IEventStore tenantStore, TenantId tenant)
    {
        var eventTypes = _meters
            .SelectMany(_ => _.EventTypes)
            .Distinct()
            .Select(tenantStore.EventTypes.GetEventTypeFor)
            .ToArray();
        if (eventTypes.Length == 0)
        {
            return;
        }

        var cursor = await store.GetCursor(tenant);
        var events = await tenantStore.EventLog.GetFromSequenceNumber(cursor, filterEventTypes: eventTypes);
        if (events.Count == 0)
        {
            return;
        }

        foreach (var appended in events.OrderBy(_ => _.Context.SequenceNumber.Value))
        {
            foreach (var meter in _meters.Where(_ => _.EventTypes.Contains(appended.Content.GetType())))
            {
                var reading = meter.Measure(appended.Content, appended.Context);
                if (reading is null)
                {
                    continue;
                }

                var alreadyClosed = (await store.Get(tenant, reading.Day))?.IsClosed ?? false;
                if (await store.Add(tenant, meter.Id, reading, appended.Context.SequenceNumber, alreadyClosed))
                {
                    metrics.BucketUpdated(meter.Id.Value);
                }
            }
        }

        await store.SetCursor(tenant, events.Max(_ => _.Context.SequenceNumber.Value) + 1UL);
    }

    async Task CloseDueDays(IEventStore tenantStore, TenantId tenant, DateTimeOffset now)
    {
        var outbox = tenantStore.GetEventSequence(EventSequenceId.Outbox);
        var through = ComputeDayExtensions.LatestClosedAt(now, options.Value.CloseDelay);

        foreach (var day in await store.GetOpen(tenant, through))
        {
            var key = new IdempotencyKey($"boxoffice-compute-closed-{tenant}-{day.Day.Value:yyyy-MM-dd}");
            var result = await outbox.AppendOnce(day.Organization, new ComputeDayClosed(tenant, day.Day, [.. day.Usage]), key);
            if (result.HasIdempotencyConflicts)
            {
                logger.AlreadyReported(tenant, day.Day);
            }
            else if (!result.IsSuccess)
            {
                throw new ComputeNotReported(tenant, day.Day, result);
            }
            else
            {
                metrics.DayClosed();
                logger.DayClosed(tenant, day.Day, day.Quantities.Count);
            }

            await store.Close(tenant, day.Day);
        }
    }

    async Task ReportCorrections(IEventStore tenantStore, TenantId tenant)
    {
        var outbox = tenantStore.GetEventSequence(EventSequenceId.Outbox);

        foreach (var day in await store.GetAwaitingCorrection(tenant))
        {
            var due = day.CorrectionDue!;
            var key = new IdempotencyKey($"boxoffice-compute-corrected-{tenant}-{day.Day.Value:yyyy-MM-dd}-{due.Value}");
            var result = await outbox.AppendOnce(day.Organization, new ComputeDayCorrected(tenant, day.Day, [.. day.Usage]), key);
            if (result.HasIdempotencyConflicts)
            {
                logger.AlreadyReported(tenant, day.Day);
            }
            else if (!result.IsSuccess)
            {
                throw new ComputeNotReported(tenant, day.Day, result);
            }
            else
            {
                metrics.DayCorrected();
                logger.DayCorrected(tenant, day.Day);
            }

            await store.Corrected(tenant, day.Day, due);
        }
    }
}
