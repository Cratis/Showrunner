// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Herald.Sending;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.for_HeraldServiceCollectionExtensions.when_adding_herald;

public class to_a_host_with_an_application_event_store : Specification
{
    string _folder;
    EventStoreName _name;

    void Because()
    {
        _folder = Directory.CreateTempSubdirectory("herald-").FullName;

        // What AddCratis registers: the client and a scoped event store, with no name in the client options.
        var applicationStore = Substitute.For<IEventStore>();
        applicationStore.Name.Returns(new EventStoreName("Ledger"));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IChronicleClient>());
        services.AddSingleton(Options.Create(new ChronicleClientOptions()));
        services.AddScoped(_ => applicationStore);
        services.AddHerald(options =>
        {
            options.TemplatesFolder = _folder;
            options.Sender.Address = "noreply@ledger.example.com";
            options.Smtp.Host = "smtp.example.com";
        });
        using var provider = services.BuildServiceProvider();
        _name = provider.GetRequiredService<HeraldEventStore>().Name;
    }

    void Destroy() => Directory.Delete(_folder);

    [Fact] void should_record_in_the_application_event_store() => Assert.Equal(new EventStoreName("Ledger"), _name);
}
#endif
