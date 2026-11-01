// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;
using Cratis.Showrunner.Herald.Transport;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_whose_retry_succeeds : given.a_queued_email
{
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        _transport.Failure = new EmailTransportException("down", permanent: false);
        await _delivery.Deliver(_eventStore, Invitation, _id);
        _transport.Failure = null;
        _time.Advance(FirstDelay);
        await _delivery.Deliver(_eventStore, Invitation, _id);
        _facts = await Facts();
    }

    [Fact] void should_record_that_it_was_sent_after_two_attempts() => Assert.Equal(2, _facts.OfType<EmailSent>().Single().Attempts);
    [Fact] void should_keep_the_failed_attempt_in_the_history() => Assert.Single(_facts.OfType<EmailAttemptFailed>());
    [Fact] void should_not_send_it_again() => Assert.Single(_transport.Accepted);
}
#endif
