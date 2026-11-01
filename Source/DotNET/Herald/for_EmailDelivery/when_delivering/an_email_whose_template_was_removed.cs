// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_whose_template_was_removed : given.a_queued_email
{
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        File.Delete(System.IO.Path.Combine(_templates, "invitation.en.html"));
        await _delivery.Deliver(_eventStore, Invitation, _id);
        _facts = await Facts();
    }

    [Fact] void should_send_nothing() => Assert.Equal(0, _transport.Offered);
    [Fact] void should_record_that_it_failed_before_any_delivery() => Assert.Equal(0, _facts.OfType<EmailFailed>().Single().Attempts);
}
#endif
