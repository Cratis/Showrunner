// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_whose_last_failure_says_to_wait : given.a_queued_email
{
    async Task Because()
    {
        await _eventStore.EventLog.Append(Invitation, new EmailAttemptFailed(_id, 1, "down", _time.GetUtcNow() - TimeSpan.FromMinutes(1)));
        await _eventStore.EventLog.Append(Invitation, new EmailAttemptFailed(_id, 2, "down", _time.GetUtcNow() + TimeSpan.FromMinutes(1)));
        await _delivery.Deliver(_eventStore, Invitation, _id);
    }

    [Fact] void should_wait_for_the_time_of_the_last_failure() => Assert.Equal(0, _transport.Offered);
}
#endif
