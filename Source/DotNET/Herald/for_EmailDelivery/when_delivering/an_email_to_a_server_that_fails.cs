// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;
using Cratis.Showrunner.Herald.Transport;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_to_a_server_that_fails : given.a_queued_email
{
    IReadOnlyList<object> _facts;
    EmailAttemptFailed _attempt;

    async Task Because()
    {
        _transport.Failure = new EmailTransportException("The mail server could not take the message.", permanent: false);
        await _delivery.Deliver(_eventStore, Invitation, _id);
        _facts = await Facts();
        _attempt = _facts.OfType<EmailAttemptFailed>().Single();
    }

    [Fact] void should_keep_the_email_queued() => Assert.Empty(_facts.OfType<EmailSent>().Cast<object>().Concat(_facts.OfType<EmailFailed>()));
    [Fact] void should_record_the_failed_attempt() => Assert.Equal(1, _attempt.Attempt);
    [Fact] void should_try_again_after_the_first_delay() => Assert.Equal(_time.GetUtcNow() + FirstDelay, _attempt.NextAttemptAt);
    [Fact] void should_say_why() => Assert.Equal("The mail server could not take the message.", _attempt.Reason.Value);
}
#endif
