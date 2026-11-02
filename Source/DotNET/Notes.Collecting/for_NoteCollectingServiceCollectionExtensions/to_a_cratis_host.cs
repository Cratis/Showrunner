// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Tenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Collecting.for_NoteCollectingServiceCollectionExtensions;

public class to_a_cratis_host : Specification
{
    IChronicleClient _client;
    IChronicleClient _explicitClient;
    bool _resolved;

    static (IChronicleClient Client, ServiceProvider Provider) Host(Action<ServiceCollection> configure)
    {
        // What AddCratis registers: the application's event store, scoped, and no name in the client options.
        var applicationStore = Substitute.For<IEventStore>();
        applicationStore.Name.Returns(new EventStoreName("Ledger"));
        var root = Substitute.For<IEventStore>();
        root.GetNamespaces(Arg.Any<CancellationToken>()).Returns(_ => [EventStoreNamespaceName.Default]);
        var client = Substitute.For<IChronicleClient>();
        client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(root);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(client);
        services.AddSingleton(Options.Create(new ChronicleClientOptions()));
        services.AddScoped(_ => applicationStore);
        configure(services);
        return (client, services.BuildServiceProvider());
    }

    async Task Because()
    {
        var (client, provider) = Host(services => services.AddNoteCollecting(_ => { }));
        await using (provider)
        {
            _client = client;
            _resolved = provider.GetRequiredService<TenantEventStores>() is not null;
            await provider.GetRequiredService<TenantEventStores>().All();
        }

        var (explicitClient, explicitProvider) = Host(services => services.AddNoteCollecting(options => options.EventStore = "Mail"));
        await using (explicitProvider)
        {
            _explicitClient = explicitClient;
            await explicitProvider.GetRequiredService<TenantEventStores>().All();
        }
    }

    [Fact] void should_provide_the_tenant_event_stores() => Assert.True(_resolved);
    [Fact] void should_open_the_application_event_store() => _client.Received().GetEventStore(new EventStoreName("Ledger"), EventStoreNamespaceName.Default);
    [Fact] void should_open_the_one_set_explicitly_in_the_options() => _explicitClient.Received().GetEventStore(new EventStoreName("Mail"), EventStoreNamespaceName.Default);
}
#endif
