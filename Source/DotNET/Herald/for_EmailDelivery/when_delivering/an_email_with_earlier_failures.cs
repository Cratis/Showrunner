// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_with_earlier_failures : given.a_queued_email
{
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        var past = _time.GetUtcNow() - TimeSpan.FromMinutes(1);
        await _eventStore.EventLog.Append(Invitation, new EmailAttemptFailed(_id, 2, "down", past));
        await _eventStore.EventLog.Append(Invitation, new EmailAttemptFailed(_id, 1, "down", past));
        await _delivery.Deliver(_eventStore, Invitation, _id);
        _facts = await Facts();
    }

    [Fact] void should_count_from_the_highest_failed_attempt() => Assert.Equal(3, _facts.OfType<EmailSent>().Single().Attempts);
}
#endif
