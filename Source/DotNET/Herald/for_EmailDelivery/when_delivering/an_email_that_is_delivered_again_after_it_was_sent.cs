// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_that_is_delivered_again_after_it_was_sent : given.a_queued_email
{
    async Task Because()
    {
        await _delivery.Deliver(_eventStore, Invitation, _id);
        await _delivery.Deliver(_eventStore, Invitation, _id);
    }

    [Fact] void should_send_it_once() => Assert.Single(_transport.Accepted);
}
#endif
