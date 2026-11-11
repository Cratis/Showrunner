// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_that_cannot_be_rendered_when_delivery_is_cancelled : given.a_queued_email
{
    IReadOnlyList<object> _facts;
    Exception _exception;

    async Task Because()
    {
        File.Delete(System.IO.Path.Combine(_templates, "invitation.en.html"));
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        _exception = await Record.ExceptionAsync(() => _delivery.Deliver(_eventStore, Invitation, _id, cancellation.Token));
        _facts = await Facts();
    }

    [Fact] void should_be_cancelled() => Assert.IsType<OperationCanceledException>(_exception, exactMatch: false);
    [Fact] void should_not_record_that_it_failed() => Assert.Empty(_facts.OfType<EmailFailed>());
}
#endif
