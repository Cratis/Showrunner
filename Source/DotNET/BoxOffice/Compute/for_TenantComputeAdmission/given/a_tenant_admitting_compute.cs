// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.Testing.Events;
using Cratis.Metrics;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Cratis.Showrunner.BoxOffice.Offering.Metering;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;

/// <summary>
/// A tenant of a product that declares an <c>agent-runs</c> meter with a concurrency limit and a <c>tokens</c> meter
/// without one, against an in-process event store and metering store.
/// </summary>
public class a_tenant_admitting_compute : Specification
{
    /// <summary>
    /// The meter with a concurrency limit.
    /// </summary>
    protected static readonly MeterId AgentRuns = new("agent-runs");

    /// <summary>
    /// The meter without a concurrency limit.
    /// </summary>
    protected static readonly MeterId Tokens = new("tokens");

    protected EventStoreForTesting _eventStore;
    protected IEntitlements _entitlements;
    protected IMeteringStore _metering;
    protected Clock _clock;
    protected BoxOfficeOptions _options;
    protected ProductOffering _offering;
    protected TenantComputeAdmission _admission;
    readonly Dictionary<ComputeDay, Dictionary<MeterId, MeterQuantity>> _usage = [];

    void Establish()
    {
        _eventStore = new();
        _entitlements = Substitute.For<IEntitlements>();
        _metering = Substitute.For<IMeteringStore>();
        _metering.Get(Arg.Any<TenantId>(), Arg.Any<ComputeDay>()).Returns(call => Task.FromResult(DayOf(call.Arg<ComputeDay>())));
        _clock = new(Tenants.Now);
        _options = new();
        _offering = new ProductOfferingBuilder()
            .Product(ProductId.New())
            .Meter(AgentRuns, "Agent runs", "runs", hasConcurrencyLimit: true)
            .Meter(Tokens, "Tokens", "tokens")
            .Build();
        _admission = CreateAdmission(_clock);
    }

    void Destroy() => _eventStore.Dispose();

    /// <summary>
    /// Creates the admission over the tenant's stores, running on a clock.
    /// </summary>
    /// <param name="clock">The clock.</param>
    /// <returns>The admission.</returns>
    protected TenantComputeAdmission CreateAdmission(TimeProvider clock) => new(
        _eventStore,
        _entitlements,
        _offering,
        _metering,
        Options.Create(_options),
        clock,
        new UnkeyedMeter<TenantComputeAdmission>(),
        Microsoft.Extensions.Logging.Abstractions.NullLogger<TenantComputeAdmission>.Instance);

    /// <summary>
    /// Says what the operator allows of a meter.
    /// </summary>
    /// <param name="meter">The meter.</param>
    /// <param name="quantity">How much the tenant may use in the period.</param>
    /// <param name="period">The period.</param>
    /// <param name="overage">What happens to work beyond the allowance.</param>
    protected void Allow(MeterId meter, long quantity, AllowancePeriod period = AllowancePeriod.Day, OverageBehavior overage = OverageBehavior.Block) =>
        _entitlements.AllowanceFor(meter).Returns(new MeterAllowance(meter, quantity, period, overage, Tenants.Start));

    /// <summary>
    /// Says what the metering measured of a meter on a day.
    /// </summary>
    /// <param name="day">The day.</param>
    /// <param name="meter">The meter.</param>
    /// <param name="quantity">The quantity.</param>
    protected void Measured(DateOnly day, MeterId meter, long quantity)
    {
        if (!_usage.TryGetValue(new(day), out var meters))
        {
            _usage[new(day)] = meters = [];
        }

        meters[meter] = quantity;
    }

    /// <summary>
    /// Records a limit the product's maintainers set.
    /// </summary>
    /// <param name="facts">The facts.</param>
    /// <returns>An awaitable task.</returns>
    protected async Task Maintainers(params object[] facts)
    {
        foreach (var fact in facts)
        {
            await _eventStore.EventLog.Append(TenantStream.Id, fact);
        }
    }

    /// <summary>
    /// Reads what is recorded of the tenant's stream.
    /// </summary>
    /// <returns>The facts, in the order they were recorded.</returns>
    protected async Task<IReadOnlyList<object>> TenantFacts() =>
        [.. (await _eventStore.EventLog.GetFromSequenceNumber(EventSequenceNumber.First)).Select(_ => _.Content)];

    MeteringDay? DayOf(ComputeDay day) => _usage.TryGetValue(day, out var meters)
        ? new MeteringDay(Tenants.Tenant, day, Tenants.Organization, false, null, meters)
        : null;
}
#endif
