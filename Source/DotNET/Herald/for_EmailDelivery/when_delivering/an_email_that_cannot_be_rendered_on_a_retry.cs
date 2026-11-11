// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_that_cannot_be_rendered_on_a_retry : given.a_queued_email
{
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        await _eventStore.EventLog.Append(Invitation, new EmailAttemptFailed(_id, 1, "down", _time.GetUtcNow() - TimeSpan.FromMinutes(1)));
        File.Delete(System.IO.Path.Combine(_templates, "invitation.en.html"));
        await _delivery.Deliver(_eventStore, Invitation, _id);
        _facts = await Facts();
    }

    [Fact] void should_send_nothing() => Assert.Equal(0, _transport.Offered);
    [Fact] void should_fail_for_good_with_the_attempts_so_far() => Assert.Equal(1, _facts.OfType<EmailFailed>().Single().Attempts);
    [Fact] void should_record_no_further_failed_attempt() => Assert.Single(_facts.OfType<EmailAttemptFailed>());
}
#endif
