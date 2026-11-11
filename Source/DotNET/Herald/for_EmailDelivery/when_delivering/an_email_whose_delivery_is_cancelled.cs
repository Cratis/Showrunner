// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;
using Cratis.Showrunner.Herald.Transport;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_whose_delivery_is_cancelled : given.a_queued_email
{
    IReadOnlyList<object> _facts;
    Exception _exception;

    async Task Because()
    {
        _transport.Failure = new EmailTransportException("down", permanent: false);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        _exception = await Record.ExceptionAsync(() => _delivery.Deliver(_eventStore, Invitation, _id, cancellation.Token));
        _facts = await Facts();
    }

    [Fact] void should_let_the_failure_through() => Assert.IsType<EmailTransportException>(_exception);
    [Fact] void should_not_count_it_as_a_failed_attempt() => Assert.Empty(_facts.OfType<EmailAttemptFailed>());
}
#endif
