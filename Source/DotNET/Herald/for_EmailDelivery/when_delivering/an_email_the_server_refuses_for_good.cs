// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;
using Cratis.Showrunner.Herald.Transport;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_the_server_refuses_for_good : given.a_queued_email
{
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        _transport.Failure = new EmailTransportException("The mail server refused the message: 550 MailboxUnavailable.", permanent: true);
        await _delivery.Deliver(_eventStore, Invitation, _id);
        _facts = await Facts();
    }

    [Fact] void should_fail_at_once() => Assert.Equal(1, _facts.OfType<EmailFailed>().Single().Attempts);
    [Fact] void should_not_try_again() => Assert.Empty(_facts.OfType<EmailAttemptFailed>());
}
#endif
