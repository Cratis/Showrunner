// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.BoxOffice.Contracts.Offers;
using Cratis.Showrunner.BoxOffice.Contracts.Seats;
using Cratis.Showrunner.BoxOffice.Contracts.Tenants;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Offering.Starting.for_DefaultOffering.given;

/// <summary>
/// A product whose default offering is Starter, with a catalog the operator published, over an in-process event store.
/// </summary>
public class a_product_with_a_default_offering : Specification
{
    protected static readonly TenantId Tenant = "fjord-logistics";
    protected static readonly OrganizationId Organization = OrganizationId.New();
    protected static readonly OfferId StarterId = OfferId.New();

    protected EventStoreForTesting _eventStore;
    protected IChronicleClient _client;
    protected List<PublishedOffer> _catalog;

    protected static ProductOffering Offering(string? defaultOffer = "Starter")
    {
        var builder = new ProductOfferingBuilder().Product(ProductId.New()).Module("core", "Core").Module("analytics", "Analytics");
        return (defaultOffer is null ? builder : builder.DefaultOffering(defaultOffer)).Build();
    }

    protected DefaultOffering StartingWith(ProductOffering offering, string? operatorAudience = null)
    {
        var offers = Substitute.For<IPublishedOffers>();
        offers.All(Arg.Any<CancellationToken>()).Returns(_ => Task.FromResult<IEnumerable<PublishedOffer>>(_catalog));
        return new(
            offering,
            offers,
            _client,
            BoxOfficeEventStore.ForClientOptions(Options.Create(new ChronicleClientOptions())),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DefaultOffering>.Instance,
            Options.Create(new OfferingOptions { OperatorAudience = operatorAudience }));
    }

    protected async Task<IImmutableList<AppendedEvent>> Told() =>
        await _eventStore.GetEventSequence(EventSequenceId.Outbox).GetFromSequenceNumber(EventSequenceNumber.First);

    protected async Task<IImmutableList<AppendedEvent>> Entitled() =>
        await _eventStore.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, TenantStreamId.Value);

    void Establish()
    {
        _eventStore = new();
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(_eventStore);
        _catalog =
        [
            new(StarterId, "Starter", [new ModuleId("core")], new SeatCount(3), false),
            new(OfferId.New(), "Enterprise", [new ModuleId("core"), new ModuleId("analytics")], new SeatCount(100), false)
        ];
    }

    void Destroy() => _eventStore.Dispose();
}
#endif
