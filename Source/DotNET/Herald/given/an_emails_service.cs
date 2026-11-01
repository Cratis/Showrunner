// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Herald.Sending;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.given;

/// <summary>
/// The service an application sends email with, over an event store that is not running a kernel.
/// </summary>
public class an_emails_service : a_templates_folder
{
    protected static readonly EventSourceId Invitation = "invitation-1";

    protected EventStoreForTesting _eventStore;
    protected Emails _emails;

    protected async Task<IReadOnlyList<object>> Facts()
    {
        var appended = await _eventStore.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, Invitation);
        return [.. appended.Select(item => item.Content)];
    }

    void Establish()
    {
        _eventStore = new();
        var client = Substitute.For<IChronicleClient>();
        client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(_eventStore);
        var wrapped = Options.Create(_options);
        _emails = new(client, new Sending.HeraldEventStore(new ServiceCollection().BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(), Options.Create(new ChronicleClientOptions()), wrapped), wrapped, _renderer, TimeProvider.System);
    }

    void Destroy() => _eventStore.Dispose();
}
#endif
