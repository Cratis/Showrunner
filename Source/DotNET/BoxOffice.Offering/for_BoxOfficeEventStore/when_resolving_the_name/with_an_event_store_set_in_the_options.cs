// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Offering.for_BoxOfficeEventStore.when_resolving_the_name;

public class with_an_event_store_set_in_the_options : Specification
{
    EventStoreName _name;

    void Because()
    {
        // An AddCratis host registers the application's IEventStore and leaves ChronicleClientOptions.EventStore empty.
        var applicationStore = Substitute.For<IEventStore>();
        applicationStore.Name.Returns(new EventStoreName("Ledger"));
        var services = new ServiceCollection();
        services.AddScoped(_ => applicationStore);
        using var provider = services.BuildServiceProvider();
        _name = new BoxOfficeEventStore(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new ChronicleClientOptions { EventStore = "Direct" }),
            Options.Create(new OfferingOptions { EventStore = "Mail" })).Name;
    }

    [Fact] void should_be_the_one_set_explicitly() => Assert.Equal(new EventStoreName("Mail"), _name);
}
#endif
