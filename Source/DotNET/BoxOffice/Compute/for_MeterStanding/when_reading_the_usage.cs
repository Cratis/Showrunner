// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.ReadModels;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Cratis.Showrunner.BoxOffice.Offering.Metering;

namespace Cratis.Showrunner.BoxOffice.Compute.for_MeterStanding;

public class when_reading_the_usage : Specification
{
    static readonly MeterId AgentRuns = new("agent-runs");
    static readonly MeterId Tokens = new("tokens");

    EventStoreForTesting _eventStore;
    IEntitlements _entitlements;
    IReadModels _readModels;
    IMeteringStore _metering;
    ProductOffering _offering;
    Clock _clock;
    IEnumerable<MeterStanding> _standings;

    void Establish()
    {
        _eventStore = new();
        _clock = new(Tenants.Now);
        _entitlements = Substitute.For<IEntitlements>();
        _entitlements.AllowanceFor(AgentRuns).Returns(new MeterAllowance(AgentRuns, 100, AllowancePeriod.Month, OverageBehavior.Park, Tenants.Start));
        _entitlements.AllowanceFor(Tokens).Returns(MeterAllowance.None(Tokens));

        _readModels = Substitute.For<IReadModels>();
        _readModels.GetInstanceById<ComputeLimits>(Arg.Any<ReadModelKey>(), Arg.Any<ReadModelSessionId>())
            .Returns(new ComputeLimits(TenantStream.Id.Value, [new MeterLimit(AgentRuns, 60L, 3)]));

        _metering = Substitute.For<IMeteringStore>();
        _metering.Get(Arg.Any<TenantId>(), Arg.Any<ComputeDay>()).Returns(call => Task.FromResult<MeteringDay?>(
            call.Arg<ComputeDay>().Value.Month == Tenants.Today.Month
                ? new MeteringDay(Tenants.Tenant, call.Arg<ComputeDay>(), Tenants.Organization, false, null, new Dictionary<MeterId, MeterQuantity> { [AgentRuns] = 10, [Tokens] = 7 })
                : null));

        _offering = new ProductOfferingBuilder()
            .Product(ProductId.New())
            .Meter(AgentRuns, "Agent runs", "runs", hasConcurrencyLimit: true)
            .Meter(Tokens, "Tokens", "tokens")
            .Build();
    }

    void Destroy() => _eventStore.Dispose();

    async Task Because() => _standings = await MeterStanding.CurrentUsage(_offering, _entitlements, _readModels, _metering, _eventStore, _clock);

    MeterStanding Of(MeterId meter) => _standings.Single(_ => _.Meter == meter);

    [Fact] void should_tell_every_declared_meter() => _standings.Count().ShouldEqual(2);
    [Fact] void should_order_the_meters_by_their_names() => _standings.Select(_ => _.Name.Value).ShouldEqual(["Agent runs", "Tokens"]);
    [Fact] void should_add_up_the_days_of_the_period_so_far() => Of(AgentRuns).Used.ShouldEqual(20L);
    [Fact] void should_tell_the_allowance() => Of(AgentRuns).Allowance.ShouldEqual(100L);
    [Fact] void should_tell_the_period() => Of(AgentRuns).Period.ShouldEqual(AllowancePeriod.Month);
    [Fact] void should_tell_what_happens_beyond_the_allowance() => Of(AgentRuns).Overage.ShouldEqual(OverageBehavior.Park);
    [Fact] void should_tell_when_the_period_ends() => Of(AgentRuns).PeriodEnds.ShouldEqual(new DateOnly(2026, 12, 1));
    [Fact] void should_tell_the_budget() => Of(AgentRuns).Budget.ShouldEqual(60L);
    [Fact] void should_tell_the_concurrency_limit() => Of(AgentRuns).ConcurrencyLimit.ShouldEqual(3);
    [Fact] void should_tell_that_the_meter_has_a_concurrency_limit() => Of(AgentRuns).HasConcurrencyLimit.ShouldBeTrue();
    [Fact] void should_tell_no_allowance_when_the_operator_set_none() => Of(Tokens).Allowance.ShouldEqual(0L);
    [Fact] void should_count_only_today_for_a_daily_period() => Of(Tokens).Used.ShouldEqual(7L);
    [Fact] void should_tell_no_budget_when_none_was_set() => Of(Tokens).Budget.ShouldBeNull();
    [Fact] void should_end_a_daily_period_tomorrow() => Of(Tokens).PeriodEnds.ShouldEqual(Tenants.Today.AddDays(1));
}
#endif
