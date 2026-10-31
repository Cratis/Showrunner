// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.given;

/// <summary>
/// An <see cref="IMeteringStore"/> in memory for specifications. It lives as long as the test, so handing it to a
/// second <see cref="TenantMetering"/> is a restart of the product that keeps its database.
/// </summary>
public class InMemoryMeteringStore : IMeteringStore
{
    readonly Lock _lock = new();
    readonly Dictionary<string, ulong> _cursors = [];
    readonly Dictionary<(string Tenant, DateOnly Day), Day> _days = [];
    string _holder = string.Empty;
    DateTimeOffset _until;

    public Task<bool> TryHold(string holder, TimeSpan lease, DateTimeOffset now)
    {
        lock (_lock)
        {
            if (_holder != holder && _until >= now)
            {
                return Task.FromResult(false);
            }

            _holder = holder;
            _until = now + lease;
            return Task.FromResult(true);
        }
    }

    public Task<EventSequenceNumber> GetCursor(TenantId tenant)
    {
        lock (_lock)
        {
            return Task.FromResult(new EventSequenceNumber(_cursors.GetValueOrDefault(tenant.Value)));
        }
    }

    public Task SetCursor(TenantId tenant, EventSequenceNumber next)
    {
        lock (_lock)
        {
            _cursors[tenant.Value] = next.Value;
            return Task.CompletedTask;
        }
    }

    public Task<MeteringDay?> Get(TenantId tenant, ComputeDay day)
    {
        lock (_lock)
        {
            return Task.FromResult(_days.TryGetValue((tenant.Value, day.Value), out var found) ? found.ToDay() : null);
        }
    }

    public Task<bool> Add(TenantId tenant, MeterId meter, MeterReading reading, EventSequenceNumber sequenceNumber, bool dueForCorrection)
    {
        lock (_lock)
        {
            if (!_days.TryGetValue((tenant.Value, reading.Day), out var day))
            {
                day = new Day(tenant, reading.Day);
                _days[(tenant.Value, reading.Day)] = day;
            }

            day.Organization = reading.Organization;
            if (day.LastApplied.TryGetValue(meter.Value, out var last) && last >= sequenceNumber.Value)
            {
                return Task.FromResult(false);
            }

            day.Quantities[meter.Value] = day.Quantities.GetValueOrDefault(meter.Value) + reading.Quantity.Value;
            day.LastApplied[meter.Value] = sequenceNumber.Value;
            if (dueForCorrection)
            {
                day.CorrectionDue = Math.Max(day.CorrectionDue ?? 0UL, sequenceNumber.Value);
            }

            return Task.FromResult(true);
        }
    }

    public Task<IReadOnlyList<MeteringDay>> GetOpen(TenantId tenant, ComputeDay through)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<MeteringDay>>([.. _days.Values
                .Where(_ => _.Tenant == tenant.Value && !_.Closed && _.Date <= through.Value)
                .OrderBy(_ => _.Date)
                .Select(_ => _.ToDay())]);
        }
    }

    public Task Close(TenantId tenant, ComputeDay day)
    {
        lock (_lock)
        {
            _days[(tenant.Value, day.Value)].Closed = true;
            return Task.CompletedTask;
        }
    }

    public Task<IReadOnlyList<MeteringDay>> GetAwaitingCorrection(TenantId tenant)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<MeteringDay>>([.. _days.Values
                .Where(_ => _.Tenant == tenant.Value && _.Closed && _.CorrectionDue is not null)
                .OrderBy(_ => _.Date)
                .Select(_ => _.ToDay())]);
        }
    }

    public Task Corrected(TenantId tenant, ComputeDay day, EventSequenceNumber due)
    {
        lock (_lock)
        {
            var found = _days[(tenant.Value, day.Value)];
            if (found.CorrectionDue == due.Value)
            {
                found.CorrectionDue = null;
            }

            return Task.CompletedTask;
        }
    }

    sealed class Day(string tenant, DateOnly date)
    {
        public string Tenant { get; } = tenant;

        public DateOnly Date { get; } = date;

        public OrganizationId Organization { get; set; } = OrganizationId.New();

        public bool Closed { get; set; }

        public ulong? CorrectionDue { get; set; }

        public Dictionary<string, long> Quantities { get; } = [];

        public Dictionary<string, ulong> LastApplied { get; } = [];

        public MeteringDay ToDay() => new(
            Tenant,
            Date,
            Organization,
            Closed,
            CorrectionDue is null ? null : new EventSequenceNumber(CorrectionDue.Value),
            Quantities.ToDictionary(_ => new MeterId(_.Key), _ => new MeterQuantity(_.Value)));
    }
}
#endif
