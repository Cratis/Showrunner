// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.ReadModels;
using Cratis.Execution;
using Cratis.Showrunner.GuestList.given;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Querying.for_GuestLists.given;

/// <summary>
/// <see cref="GuestLists"/> for the product <c>Ledger</c> over read models that fold the facts the product recorded: the
/// system's in its own namespace and, in the namespace of the tenant <c>acme</c>, the tenant's and its users'. Nothing here
/// talks to the operator.
/// </summary>
public class a_guest_list : Specification
{
    protected static readonly EventStoreName ProductStore = new("Ledger");

    protected GuestLists _guestList;
    IReadModels _productReadModels;
    IReadModels _acmeReadModels;
    ulong _sequenceNumber;

    void Establish()
    {
        _productReadModels = Substitute.For<IReadModels>();
        _acmeReadModels = Substitute.For<IReadModels>();
        _productReadModels.GetInstanceById<AudienceHoldings>(Arg.Any<ReadModelKey>(), Arg.Any<ReadModelSessionId>()).Returns(Task.FromResult<AudienceHoldings>(null!));
        _acmeReadModels.GetInstanceById<AudienceHoldings>(Arg.Any<ReadModelKey>(), Arg.Any<ReadModelSessionId>()).Returns(Task.FromResult<AudienceHoldings>(null!));
        _acmeReadModels.GetInstances<AudienceHoldings>().Returns(Task.FromResult<IEnumerable<AudienceHoldings>>([]));
        var productStore = StoreWith(_productReadModels);
        var acmeStore = StoreWith(_acmeReadModels);
        var client = Substitute.For<IChronicleClient>();
        client.GetEventStore(ProductStore, Arg.Is<EventStoreNamespaceName?>(_ => _ == EventStoreNamespaceName.Default)).Returns(productStore);
        client.GetEventStore(ProductStore, Arg.Is<EventStoreNamespaceName?>(_ => _ == new EventStoreNamespaceName(Guests.Acme.Value))).Returns(acmeStore);
        _guestList = new(client, Options.Create(new ChronicleClientOptions { EventStore = ProductStore }));
    }

    /// <summary>
    /// Makes the system hold what the facts, folded in order, say.
    /// </summary>
    /// <param name="facts">The facts, with the minute the operator decided each.</param>
    protected void SystemRecorded(params (int Minute, object Fact)[] facts) =>
        _productReadModels.GetInstanceById<AudienceHoldings>(Arg.Any<ReadModelKey>(), Arg.Any<ReadModelSessionId>()).Returns(Task.FromResult(Fold(GuestListStreams.System, facts)!));

    /// <summary>
    /// Makes the tenant hold what the facts, folded in order, say.
    /// </summary>
    /// <param name="facts">The facts, with the minute the operator decided each.</param>
    protected void TenantRecorded(params (int Minute, object Fact)[] facts) => Hold(GuestListStreams.Tenant, facts);

    /// <summary>
    /// Makes a user of the tenant hold what the facts, folded in order, say.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="facts">The facts, with the minute the operator decided each.</param>
    protected void UserRecorded(UserId user, params (int Minute, object Fact)[] facts) => Hold(GuestListStreams.User(user), facts);

    static IEventStore StoreWith(IReadModels readModels)
    {
        var store = Substitute.For<IEventStore>();
        store.ReadModels.Returns(readModels);
        return store;
    }

    readonly Dictionary<string, AudienceHoldings> _acme = [];

    void Hold(EventSourceId stream, (int Minute, object Fact)[] facts)
    {
        var holdings = Fold(stream, facts)!;
        _acme[stream.Value] = holdings;
        _acmeReadModels.GetInstanceById<AudienceHoldings>(Arg.Is<ReadModelKey>(key => key.Value == stream.Value), Arg.Any<ReadModelSessionId>()).Returns(Task.FromResult(holdings));
        _acmeReadModels.GetInstances<AudienceHoldings>().Returns(Task.FromResult<IEnumerable<AudienceHoldings>>([.. _acme.Values]));
    }

    AudienceHoldings? Fold(EventSourceId stream, (int Minute, object Fact)[] facts)
    {
        AudienceHoldings? current = null;
        foreach (var (minute, fact) in facts)
        {
            var context = EventContext.From(
                EventStoreName.NotSet,
                EventStoreNamespaceName.NotSet,
                EventType.Unknown,
                EventSourceType.Default,
                stream,
                EventStreamType.All,
                EventStreamId.Default,
                _sequenceNumber++,
                CorrelationId.New(),
                Guests.At(minute));
            current = fact switch
            {
                AudienceGrantRecorded grant => AudienceHoldings.Reduce(grant, current, context),
                AudienceWithdrawalRecorded withdrawal => AudienceHoldings.Reduce(withdrawal, current, context),
                _ => current
            };
        }

        return current;
    }
}
#endif
