// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Transport;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_that_is_not_due_yet : given.a_queued_email
{
    int _offeredAfterTheFirst;

    async Task Because()
    {
        _transport.Failure = new EmailTransportException("down", permanent: false);
        await _delivery.Deliver(_eventStore, Invitation, _id);
        _offeredAfterTheFirst = _transport.Offered;
        _time.Advance(FirstDelay - TimeSpan.FromSeconds(1));
        await _delivery.Deliver(_eventStore, Invitation, _id);
    }

    [Fact] void should_have_tried_once() => Assert.Equal(1, _offeredAfterTheFirst);
    [Fact] void should_wait_until_it_is_due() => Assert.Equal(1, _transport.Offered);
}
#endif
