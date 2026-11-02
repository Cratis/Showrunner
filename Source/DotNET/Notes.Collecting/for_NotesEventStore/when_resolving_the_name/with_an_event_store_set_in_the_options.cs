// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Tenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Collecting.for_NotesEventStore.when_resolving_the_name;

public class with_an_event_store_set_in_the_options : Specification
{
    EventStoreName _name;

    void Because()
    {
        var applicationStore = Substitute.For<IEventStore>();
        applicationStore.Name.Returns(new EventStoreName("Ledger"));
        var services = new ServiceCollection();
        services.AddScoped(_ => applicationStore);
        services.AddSingleton(Options.Create(new ChronicleClientOptions()));
        services.AddSingleton(Options.Create(new NoteCollectingOptions { EventStore = "Studio" }));
        services.AddSingleton<NotesEventStore>();
        using var provider = services.BuildServiceProvider();
        _name = provider.GetRequiredService<NotesEventStore>().Name;
    }

    [Fact] void should_be_the_one_set_explicitly() => Assert.Equal(new EventStoreName("Studio"), _name);
}
#endif
