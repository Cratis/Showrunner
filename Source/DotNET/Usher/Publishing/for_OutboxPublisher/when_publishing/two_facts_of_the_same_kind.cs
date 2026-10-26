// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Conversation.AddDetails;
using Cratis.Showrunner.Usher.Publishing.for_OutboxPublisher.given;

namespace Cratis.Showrunner.Usher.Publishing.for_OutboxPublisher.when_publishing;

public class two_facts_of_the_same_kind : an_outbox_publisher
{
    async Task Because()
    {
        var first = ContextFor(3);
        var second = ContextFor(4);
        await _publisher.AddedDetails(new ReporterAddedDetails("It happens every time."), first, DeliveryOf(first));
        await _publisher.AddedDetails(new ReporterAddedDetails("It happens every time."), second, DeliveryOf(second));
        await ReadOutbox();
    }

    [Fact] void should_publish_both() => _outbox.Count.ShouldEqual(2);
}
#endif
