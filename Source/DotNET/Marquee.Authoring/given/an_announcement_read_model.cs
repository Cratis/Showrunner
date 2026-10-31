// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Execution;

namespace Cratis.Showrunner.Marquee.Authoring.given;

/// <summary>
/// A read model scenario for the announcement read model, where events happen on an announcement one minute apart.
/// </summary>
public class an_announcement_read_model : Specification
{
    /// <summary>
    /// When the first event happens.
    /// </summary>
    protected static readonly DateTimeOffset Start = new(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);

    protected ReadModelScenario<Announcement> _scenario;
    protected AnnouncementId _announcementId;
    ulong _sequenceNumber;

    void Establish()
    {
        _scenario = new();
        _announcementId = AnnouncementId.New();
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Lets events happen on the announcement, after any that happened before.
    /// </summary>
    /// <param name="events">The events, in the order they happen; arrays of events are flattened.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Happened(params object[] events) =>
        _scenario.ProcessAppendedEvents([.. AnnouncementEvents.Then(events).Select(@event =>
        {
            var sequenceNumber = _sequenceNumber++;
            var context = EventContext.From(
                EventStoreName.NotSet,
                EventStoreNamespaceName.NotSet,
                EventType.Unknown,
                EventSourceType.Default,
                _announcementId,
                EventStreamType.All,
                EventStreamId.Default,
                sequenceNumber,
                CorrelationId.New(),
                Start.AddMinutes(sequenceNumber));
            return new AppendedEvent(context, @event);
        })]);
}
#endif
