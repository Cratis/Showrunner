// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.ReadModels;
using Cratis.Chronicle.Transactions;
using Cratis.Showrunner.BoxOffice.Offering;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Cratis.Showrunner.BoxOffice.Offering.Starting;
using Cratis.Showrunner.BoxOffice.Offers;
using Cratis.Showrunner.BoxOffice.Tenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cratis.Showrunner.BoxOffice.for_BoxOfficeServiceCollectionExtensions.when_adding_box_office;

public class to_a_cratis_host : Specification
{
    EventStoreName _name;
    EventStoreName _explicit;
    bool _offersResolved;
    bool _tenantStoresResolved;
    bool _reportingResolved;
    bool _defaultOfferingResolved;

    void Because()
    {
        // What AddCratis registers: the application's event store, scoped, and no name in the client options.
        var applicationStore = Substitute.For<IEventStore>();
        applicationStore.Name.Returns(new EventStoreName("Ledger"));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IChronicleClient>());
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new ChronicleClientOptions()));
        services.AddSingleton(Substitute.For<IReadModels>());
        services.AddSingleton(Substitute.For<IUnitOfWorkManager>());
        services.AddScoped(_ => applicationStore);
        services.AddBoxOfficeOffering(offering => offering.Product(ProductId.New()).Module("core", "Core").DefaultOffering("Starter"));
        services.AddBoxOffice();
        using var provider = services.BuildServiceProvider();
        _name = provider.GetRequiredService<BoxOfficeEventStore>().Name;
        _offersResolved = provider.GetRequiredService<ProductOffers>() is not null;
        _tenantStoresResolved = provider.GetRequiredService<TenantEventStores>() is not null;
        _reportingResolved = provider.GetServices<IHostedService>().OfType<OfferingReportingService>().Any();
        _defaultOfferingResolved = provider.GetRequiredService<IDefaultOffering>() is not null;

        var another = new ServiceCollection();
        another.AddLogging();
        another.AddSingleton(Substitute.For<IChronicleClient>());
        another.AddSingleton(Microsoft.Extensions.Options.Options.Create(new ChronicleClientOptions()));
        another.AddScoped(_ => applicationStore);
        another.AddBoxOffice(options => options.EventStore = "Mail");
        using var anotherProvider = another.BuildServiceProvider();
        _explicit = anotherProvider.GetRequiredService<BoxOfficeEventStore>().Name;
    }

    [Fact] void should_use_the_application_event_store() => Assert.Equal(new EventStoreName("Ledger"), _name);
    [Fact] void should_use_the_one_set_explicitly_in_box_office_options() => Assert.Equal(new EventStoreName("Mail"), _explicit);
    [Fact] void should_provide_the_offers() => Assert.True(_offersResolved);
    [Fact] void should_provide_the_tenant_event_stores() => Assert.True(_tenantStoresResolved);
    [Fact] void should_report_the_offering() => Assert.True(_reportingResolved);
    [Fact] void should_start_tenants_on_the_default_offering() => Assert.True(_defaultOfferingResolved);
}
#endif
