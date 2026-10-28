// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Conversation.AddDetails;
using Cratis.Showrunner.Usher.Publishing.for_OutboxPublisher.given;

namespace Cratis.Showrunner.Usher.Publishing.for_OutboxPublisher.when_publishing;

public class the_same_fact_twice : an_outbox_publisher
{
    async Task Because()
    {
        var context = ContextFor(3);
        var delivery = DeliveryOf(context);
        await _publisher.AddedDetails(new ReporterAddedDetails("It happens every time."), context, delivery);
        await _publisher.AddedDetails(new ReporterAddedDetails("It happens every time."), context, delivery);
        await ReadOutbox();
    }

    [Fact] void should_publish_it_once() => _outbox.Count.ShouldEqual(1);
}
#endif
