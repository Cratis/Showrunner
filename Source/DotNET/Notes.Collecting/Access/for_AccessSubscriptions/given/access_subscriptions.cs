// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.EventStoreSubscriptions;
using Cratis.Chronicle.Reactors;
using Cratis.Showrunner.Notes.Collecting.given;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Collecting.Access.for_AccessSubscriptions.given;

/// <summary>
/// The connection to the access stores of an application whose event store is a substitute.
/// </summary>
public class access_subscriptions : Specification
{
    protected IEventStore _eventStore;
    protected IReactors _reactors;
    protected IEventStoreSubscriptions _subscriptions;
    protected AccessSubscriptions _connection;

    /// <summary>
    /// Gets the access stores the collecting store is configured with.
    /// </summary>
    protected virtual IEnumerable<string> Stores => ["Direct", "Desk2"];

    void Establish()
    {
        _reactors = Substitute.For<IReactors>();
        _subscriptions = Substitute.For<IEventStoreSubscriptions>();
        _eventStore = Substitute.For<IEventStore>();
        _eventStore.Reactors.Returns(_reactors);
        _eventStore.Subscriptions.Returns(_subscriptions);
        _eventStore.EventTypes.Returns(Substitute.For<IEventTypes>());
        _connection = new(
            new AccessEventTranslator(Substitute.For<IChronicleClient>(), new RecordingLogger<AccessEventTranslator>()),
            Options.Create(new NoteCollectingOptions { AccessStores = [.. Stores.Select(_ => new EventStoreName(_))] }));
    }
}
#endif
