// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Publishing.for_ResponsePublisher.given;
using Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;

namespace Cratis.Showrunner.Notes.Collecting.Publishing.for_ResponsePublisher.when_publishing;

/// <summary>
/// Chronicle delivers at least once, so a delivery that is repeated publishes nothing the second time.
/// </summary>
public class the_same_answer_twice : a_response_publisher
{
    async Task Because()
    {
        var context = ContextFor(3);
        var delivery = DeliveryOf(context);
        await _publisher.Acknowledged(new OperatorAcknowledgedNote(), context, delivery);
        await _publisher.Acknowledged(new OperatorAcknowledgedNote(), context, delivery);
        await ReadOutbox();
    }

    [Fact] void should_publish_it_once() => _outbox.Count.ShouldEqual(1);
}
#endif
