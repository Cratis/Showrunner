// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;
using Cratis.Showrunner.Herald.Transport;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_that_keeps_failing : given.a_queued_email
{
    IReadOnlyList<object> _facts;
    EmailFailed _failed;

    async Task Because()
    {
        _transport.Failure = new EmailTransportException("down", permanent: false);
        for (var attempt = 0; attempt < 5; attempt++)
        {
            await _delivery.Deliver(_eventStore, Invitation, _id);
            _time.Advance(TimeSpan.FromHours(2));
        }

        _facts = await Facts();
        _failed = _facts.OfType<EmailFailed>().Single();
    }

    [Fact] void should_try_as_many_times_as_allowed() => Assert.Equal(3, _transport.Offered);
    [Fact] void should_record_that_it_failed_for_good() => Assert.Equal(3, _failed.Attempts);
    [Fact] void should_say_why() => Assert.Equal("down", _failed.Reason.Value);
    [Fact] void should_not_record_that_it_was_sent() => Assert.Empty(_facts.OfType<EmailSent>());
}
#endif
