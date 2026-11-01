// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.given;

namespace Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.when_publishing;

/// <summary>
/// Two suggested property changes in one picking session are two facts and are both published.
/// </summary>
public class two_facts_of_the_same_kind : an_outbox_publisher
{
    async Task Because()
    {
        var first = ContextFor(1);
        var second = ContextFor(2);
        await _publisher.SuggestedPropertyChange(NoteEvents.Suggested(), first, DeliveryOf(first));
        await _publisher.SuggestedPropertyChange(NoteEvents.Suggested() with { Property = "color" }, second, DeliveryOf(second));
        await ReadOutbox();
    }

    [Fact] void should_publish_both() => _outbox.Count.ShouldEqual(2);
}
#endif
