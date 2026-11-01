// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.BoxOffice.Contracts.Offers;
using Cratis.Showrunner.BoxOffice.Offering.Starting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Offers.for_ProductOffers.when_reading_the_published_offers;

public class of_the_catalog : Specification
{
    static readonly OfferId StarterId = OfferId.New();

    IEnumerable<PublishedOffer> _offers;
    bool _registered;

    async Task Because()
    {
        var readModels = Substitute.For<IReadModels>();
        readModels.GetInstances<Offer>().Returns(Task.FromResult<IEnumerable<Offer>>([new Offer(StarterId, "Starter", [new ModuleId("core")], new SeatCount(3), [], true)]));
        var eventStore = Substitute.For<IEventStore>();
        eventStore.ReadModels.Returns(readModels);
        var client = Substitute.For<IChronicleClient>();
        client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(eventStore);

        var services = new ServiceCollection();
        services.AddSingleton(client);
        services.AddSingleton(Options.Create(new ChronicleClientOptions()));
        services.AddBoxOffice();
        _registered = services.Any(_ => _.ServiceType == typeof(IPublishedOffers));
        _offers = await ((IPublishedOffers)new ProductOffers(client, Options.Create(new ChronicleClientOptions()))).All();
    }

    [Fact] void should_be_available_to_the_default_offering() => _registered.ShouldBeTrue();
    [Fact] void should_describe_each_offer() => _offers.Count().ShouldEqual(1);
    [Fact] void should_keep_the_id() => _offers.Single().Id.ShouldEqual(StarterId);
    [Fact] void should_keep_the_name() => _offers.Single().Name.Value.ShouldEqual("Starter");
    [Fact] void should_keep_the_seats() => _offers.Single().IncludedSeats.Value.ShouldEqual(3);
    [Fact] void should_keep_that_it_was_withdrawn() => _offers.Single().Withdrawn.ShouldBeTrue();
}
#endif
