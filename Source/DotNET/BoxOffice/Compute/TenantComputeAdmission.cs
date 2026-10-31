// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.EventSequences.Concurrency;
using Cratis.Metrics;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Cratis.Showrunner.BoxOffice.Offering.Metering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Compute;

/// <summary>
/// Decides whether the current tenant may start a unit of work from what the operator allows, what the product's
/// maintainers budgeted and what the tenant is running right now.
/// </summary>
/// <remarks>
/// <para>
/// A unit of work is turned away, in this order, when the period's usage has reached the allowance, when it has reached
/// the tenant's budget, and when the tenant already runs as many units of work of the meter as its concurrency limit
/// allows. Turned away is <see cref="AdmissionOutcome.AllowanceExhausted"/> or <see cref="AdmissionOutcome.BudgetReached"/>
/// when the allowance's overage is <see cref="OverageBehavior.Block"/>, and <see cref="AdmissionOutcome.Parked"/> until the
/// next period when it is <see cref="OverageBehavior.Park"/>.
/// </para>
/// <para>
/// The concurrency limit is decided from the tenant's own start and stop facts, never from a projection: the facts are read
/// from the event log and the start is appended guarded by a concurrency scope on them, so two units of work asking for
/// the last slot cannot both get it. A unit of work that is never ended stops counting when its lease ends.
/// </para>
/// <para>
/// The first refusal for an allowance in a period records that the allowance ran out, once. Later refusals are logged and
/// counted, and record nothing.
/// </para>
/// </remarks>
/// <param name="eventStore">The <see cref="IEventStore"/> of the current tenant's namespace.</param>
/// <param name="entitlements">The <see cref="IEntitlements"/> of the current tenant.</param>
/// <param name="offering">The product's offering, which says what meters it declared.</param>
/// <param name="metering">The <see cref="IMeteringStore"/> holding what was measured.</param>
/// <param name="options">The <see cref="BoxOfficeOptions"/>.</param>
/// <param name="time">The <see cref="TimeProvider"/> the periods are counted by.</param>
/// <param name="metrics">The metrics.</param>
/// <param name="logger">The logger.</param>
public class TenantComputeAdmission(
    IEventStore eventStore,
    IEntitlements entitlements,
    ProductOffering offering,
    IMeteringStore metering,
    IOptions<BoxOfficeOptions> options,
    TimeProvider time,
    IMeter<TenantComputeAdmission> metrics,
    ILogger<TenantComputeAdmission> logger) : IComputeAdmission
{
    const int MaximumAttempts = 3;

    TenantId Tenant => new(eventStore.Namespace.Value);

    /// <inheritdoc/>
    public async Task<AdmissionDecision> Admit(MeterId meter)
    {
        var definition = offering.Meters.FirstOrDefault(_ => _.Id == meter) ?? throw new MeterNotDeclared(meter);
        var now = time.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        var allowance = await entitlements.AllowanceFor(meter);
        var limits = await ReadLimits(meter);
        var used = await UsedInPeriod(meter, allowance.Period, today);

        if (used >= allowance.Quantity.Value)
        {
            await RecordAllowanceReached(meter, today, allowance);
            return TurnAway(meter, AdmissionOutcome.AllowanceExhausted, allowance, today);
        }

        if (limits.Budget is { } budget && used >= budget.Value)
        {
            return TurnAway(meter, AdmissionOutcome.BudgetReached, allowance, today);
        }

        if (definition.HasConcurrencyLimit && limits.ConcurrencyLimit is { } limit)
        {
            return await StartWithinLimit(meter, limit, now);
        }

        metrics.Admitted(meter.Value);
        return AdmissionDecision.Admitted();
    }

    /// <inheritdoc/>
    public async Task End(ComputeWork work)
    {
        var stream = WorkStream(work.Meter, DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime));
        var result = await eventStore.EventLog.AppendOnce(
            stream,
            new ComputeWorkStopped(work.Meter, work.Id),
            new IdempotencyKey($"boxoffice-compute-stopped-{work.Id.Value:N}"));

        if (!result.IsSuccess && !result.HasIdempotencyConflicts && !result.WasAlreadyAppended)
        {
            throw new ComputeFactNotRecorded(typeof(ComputeWorkStopped), stream, eventStore.Namespace, result);
        }
    }

    static EventSourceId WorkStream(MeterId meter, DateOnly day) => new($"compute-{meter.Value}-{day:yyyy-MM}");

    static DateTimeOffset NextPeriodStart(AllowancePeriod period, DateOnly today) =>
        new(MeterConsumption.NextPeriodStart(period, today).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    static DateOnly PeriodStart(AllowancePeriod period, DateOnly today) => MeterConsumption.PeriodStart(period, today);

    static int Running(IEnumerable<AppendedEvent> facts, MeterId meter, DateTimeOffset now)
    {
        var content = facts.Select(_ => _.Content).ToArray();
        var stopped = content.OfType<ComputeWorkStopped>().Where(_ => _.Meter == meter).Select(_ => _.Work).ToHashSet();
        return content.OfType<ComputeWorkStarted>().Count(_ => _.Meter == meter && _.Until > now && !stopped.Contains(_.Work));
    }

    AdmissionDecision TurnAway(MeterId meter, AdmissionOutcome because, MeterAllowance allowance, DateOnly today)
    {
        metrics.Refused(meter.Value, because.ToString());
        if (allowance.Overage == OverageBehavior.Park)
        {
            var until = NextPeriodStart(allowance.Period, today);
            logger.Parked(meter, Tenant, until, because);
            return AdmissionDecision.Parked(until, because);
        }

        logger.TurnedAway(meter, Tenant, because);
        return AdmissionDecision.Refused(because);
    }

    async Task<(MeterQuantity? Budget, int? ConcurrencyLimit)> ReadLimits(MeterId meter)
    {
        var eventTypes = new[] { typeof(ComputeBudgetSet), typeof(ComputeConcurrencyLimitSet) }.Select(eventStore.EventTypes.GetEventTypeFor).ToArray();
        var facts = await eventStore.EventLog.GetForEventSourceIdAndEventTypes(TenantStream.Id, eventTypes);

        MeterQuantity? budget = null;
        int? limit = null;
        foreach (var fact in facts.Select(_ => _.Content))
        {
            switch (fact)
            {
                case ComputeBudgetSet set when set.Meter == meter:
                    budget = set.Quantity;
                    break;
                case ComputeConcurrencyLimitSet set when set.Meter == meter:
                    limit = set.Limit;
                    break;
            }
        }

        return (budget, limit);
    }

    Task<long> UsedInPeriod(MeterId meter, AllowancePeriod period, DateOnly today) =>
        MeterConsumption.UsedInPeriod(metering, Tenant, meter, period, today);

    async Task RecordAllowanceReached(MeterId meter, DateOnly today, MeterAllowance allowance)
    {
        var key = new IdempotencyKey($"boxoffice-allowance-reached-{Tenant.Value}-{meter.Value}-{PeriodStart(allowance.Period, today):yyyy-MM-dd}");
        var result = await eventStore.EventLog.AppendOnce(
            TenantStream.Id,
            new ComputeAllowanceReached(meter, new ComputeDay(today), allowance.Period, allowance.Overage),
            key);

        if (!result.IsSuccess && !result.HasIdempotencyConflicts && !result.WasAlreadyAppended)
        {
            throw new ComputeFactNotRecorded(typeof(ComputeAllowanceReached), TenantStream.Id, eventStore.Namespace, result);
        }
    }

    async Task<AdmissionDecision> StartWithinLimit(MeterId meter, int limit, DateTimeOffset now)
    {
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var stream = WorkStream(meter, today);
        var previous = WorkStream(meter, today.AddMonths(-1));
        var eventTypes = new[] { typeof(ComputeWorkStarted), typeof(ComputeWorkStopped) }.Select(eventStore.EventTypes.GetEventTypeFor).ToArray();

        for (var attempt = 1; attempt <= MaximumAttempts; attempt++)
        {
            var current = await eventStore.EventLog.GetForEventSourceIdAndEventTypes(stream, eventTypes);
            var before = await eventStore.EventLog.GetForEventSourceIdAndEventTypes(previous, eventTypes);
            if (Running([.. current, .. before], meter, now) >= limit)
            {
                metrics.Refused(meter.Value, nameof(AdmissionOutcome.AtConcurrencyLimit));
                logger.TurnedAway(meter, Tenant, AdmissionOutcome.AtConcurrencyLimit);
                return AdmissionDecision.Refused(AdmissionOutcome.AtConcurrencyLimit);
            }

            var work = new ComputeWork(meter, ComputeWorkId.New());
            var until = time.GetUtcNow() + options.Value.MaximumWorkDuration;
            var last = current.Count > 0 ? current[^1].Context.SequenceNumber : EventSequenceNumber.Unavailable;
            var result = await eventStore.EventLog.Append(
                stream,
                new ComputeWorkStarted(meter, work.Id, until),
                concurrencyScope: new ConcurrencyScope(last, stream, EventTypes: eventTypes));

            if (result.HasConcurrencyViolations)
            {
                // Other work started or ended between the read and the append; decide again on what is running now.
                continue;
            }

            if (!result.IsSuccess)
            {
                throw new ComputeFactNotRecorded(typeof(ComputeWorkStarted), stream, eventStore.Namespace, result);
            }

            metrics.Admitted(meter.Value);
            return AdmissionDecision.Admitted(work);
        }

        logger.Contended(meter, Tenant);
        metrics.Refused(meter.Value, nameof(AdmissionOutcome.AtConcurrencyLimit));
        return AdmissionDecision.Refused(AdmissionOutcome.AtConcurrencyLimit);
    }
}
