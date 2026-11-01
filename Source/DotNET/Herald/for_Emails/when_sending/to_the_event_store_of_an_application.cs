// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Herald.Sending;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.for_Emails.when_sending;

public class to_the_event_store_of_an_application : given.an_emails_service
{
    IChronicleClient _client;
    IChronicleClient _legacyClient;

    async Task Because()
    {
        // What AddCratis registers: the application's event store, with no name in the client options.
        var applicationStore = Substitute.For<IEventStore>();
        applicationStore.Name.Returns(new EventStoreName("Ledger"));
        var services = new ServiceCollection();
        services.AddScoped(_ => applicationStore);
        var scopes = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();

        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(_eventStore);
        var wrapped = Options.Create(_options);
        var emails = new Emails(_client, Options.Create(new ChronicleClientOptions()), wrapped, _renderer, TimeProvider.System, new HeraldEventStore(scopes, Options.Create(new ChronicleClientOptions()), wrapped));
        await emails.Send(new EmailRequest("invitation", Invitation, "grace@example.com", InvitationValues));

        // The constructor written before the application's store was resolved keeps working.
        _legacyClient = Substitute.For<IChronicleClient>();
        _legacyClient.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(_eventStore);
        var legacy = new Emails(_legacyClient, Options.Create(new ChronicleClientOptions { EventStore = "Mail" }), wrapped, _renderer, TimeProvider.System);
        await legacy.Send(new EmailRequest("invitation", Invitation, "grace@example.com", InvitationValues));
    }

    [Fact] void should_record_in_the_application_event_store() => _client.Received(1).GetEventStore(new EventStoreName("Ledger"), Arg.Any<EventStoreNamespaceName?>());
    [Fact] void should_keep_naming_the_store_from_the_client_options_through_the_earlier_constructor() => _legacyClient.Received(1).GetEventStore(new EventStoreName("Mail"), Arg.Any<EventStoreNamespaceName?>());
}
#endif
