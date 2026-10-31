// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Metrics;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.given;

/// <summary>
/// A tenant of a product that measures compute from its sessions, against an in-process event store and a store of
/// buckets that outlives a restart of the product.
/// </summary>
public class a_tenant_being_measured : Specification
{
    protected static readonly DateTimeOffset _noon = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);
    protected EventStoreForTesting _eventStore;
    protected InMemoryMeteringStore _store;
    protected TenantId _tenant;
    protected OrganizationId _organization;
    protected IImmutableList<AppendedEvent> _reported;
    protected System.Diagnostics.Metrics.Meter _instruments;

    void Establish()
    {
        _eventStore = new();
        _store = new();
        _tenant = "acme";
        _organization = OrganizationId.New();
        _instruments = new($"boxoffice-metering-spec-{Guid.NewGuid():N}");
    }

    void Destroy()
    {
        _instruments.Dispose();
        _eventStore.Dispose();
    }

    /// <summary>
    /// Creates the metering, as one run of the product would, over the same buckets as the runs before it.
    /// </summary>
    /// <returns>The metering.</returns>
    protected TenantMetering StartTheProduct() => new(
        _store,
        [new EventMeter<SessionEnded>("compute", (@event, _) => new MeterReading(@event.Organization, DateOnly.FromDateTime(@event.Started.UtcDateTime), @event.Minutes))],
        Options.Create(new MeteringOptions()),
        new Meter<TenantMetering>(_instruments),
        Microsoft.Extensions.Logging.Abstractions.NullLogger<TenantMetering>.Instance);

    /// <summary>
    /// Records a session that ended, which used compute.
    /// </summary>
    /// <param name="started">When the session started.</param>
    /// <param name="minutes">The minutes it used.</param>
    /// <returns>An awaitable task.</returns>
    protected Task SessionEnded(DateTimeOffset started, long minutes) =>
        _eventStore.EventLog.Append(Guid.NewGuid().ToString(), new SessionEnded(_organization, started, minutes));

    /// <summary>
    /// Reads what the product reported to the operator.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadReported() =>
        _reported = await _eventStore.GetEventSequence(EventSequenceId.Outbox).GetFromSequenceNumber(EventSequenceNumber.First);

    /// <summary>
    /// Gets the reported events of a kind.
    /// </summary>
    /// <typeparam name="TEvent">The kind.</typeparam>
    /// <returns>The events.</returns>
    protected IEnumerable<TEvent> Reported<TEvent>() => _reported.Select(_ => _.Content).OfType<TEvent>();
}
#endif
