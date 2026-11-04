// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.ReadModels;
using Cratis.Execution;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Provisioning;
using Cratis.Showrunner.BoxOffice.Product.Tenancy;
using Cratis.Showrunner.BoxOffice.Receiving;
using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.Product.given;

/// <summary>
/// A tenant the product has, whose read models say what the operator decided and who holds seats.
/// </summary>
public class a_tenant_in_the_product : Specification
{
    protected Clock _clock;
    protected IReadModels _readModels;
    protected IEventStore _store;
    protected ITenantStores _stores;

    protected static EventContext ContextAt(DateTimeOffset at) => EventContext.From(
        "Ledger",
        Tenants.Tenant.Value,
        EventType.Unknown,
        EventSourceType.Default,
        TenantStream.Id,
        EventStreamType.All,
        EventStreamId.Default,
        0,
        CorrelationId.New(),
        at);

    void Establish()
    {
        _clock = new(Tenants.Now);
        _readModels = Substitute.For<IReadModels>();
        _store = Substitute.For<IEventStore>();
        _store.ReadModels.Returns(_readModels);
        _stores = Substitute.For<ITenantStores>();
        _stores.For(Tenants.Tenant, Arg.Any<CancellationToken>()).Returns(_store);
    }

    protected void Answer<T>(T value)
        where T : class => _readModels.GetInstanceById<T>(Arg.Any<ReadModelKey>(), Arg.Any<ReadModelSessionId>()).Returns(value);

    protected void SeatsGranted(int seats) =>
        Answer(SeatCapacity.Reduce(new OperatorSetSeatCapacity(seats, EntitlementEnd.Never, SeatCapacitySource.Grant), null, ContextAt(Tenants.Start)));

    protected void TrialEndedYesterday() =>
        Answer(SeatCapacity.Reduce(new OperatorOpenedTrial(new(Tenants.Day(-1)), 3), null, ContextAt(Tenants.Start)));

    protected void TrialRunning() =>
        Answer(SeatCapacity.Reduce(new OperatorOpenedTrial(new(Tenants.Day(10)), 3), null, ContextAt(Tenants.Start)));

    protected void Holding(params Person[] people) =>
        Answer(new SeatRoster(TenantStream.Id.Value, [.. people.Select((person, index) => new SeatHolding(person, Tenants.At(index)))]));

    protected void Suspended() =>
        Answer(TenantStanding.Reduce(new OperatorSuspendedTenant(SuspensionCause.AdministratorDecision), null, ContextAt(Tenants.Start)));

    protected void CreatedBy(Person person) =>
        Answer(new TenantCreator(TenantStream.Id.Value, person, "Maintainer"));
}
#endif
