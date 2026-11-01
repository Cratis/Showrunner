// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_to_a_working_server : given.a_queued_email
{
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        await _delivery.Deliver(_eventStore, Invitation, _id);
        _facts = await Facts();
    }

    [Fact] void should_hand_one_message_to_the_transport() => Assert.Single(_transport.Accepted);
    [Fact] void should_send_it_to_the_recipient() => Assert.Equal("grace@example.com", _transport.Accepted[0].To.Address);
    [Fact] void should_send_it_from_the_sender() => Assert.Equal("noreply@ledger.example.com", _transport.Accepted[0].From.Address);
    [Fact] void should_send_the_rendered_subject() => Assert.Equal("Ada invited you to Ledger", _transport.Accepted[0].Subject);
    [Fact] void should_send_html_and_plain_text() => Assert.True(_transport.Accepted[0].Html.Length > 0 && _transport.Accepted[0].Text.Length > 0);
    [Fact] void should_send_the_image() => Assert.Single(_transport.Accepted[0].Images);
    [Fact] void should_record_that_it_was_sent() => Assert.Equal(new EmailSent(_id, "invitation", "en", 1), _facts.OfType<EmailSent>().Single());
    [Fact] void should_not_record_a_failure() => Assert.Empty(_facts.OfType<EmailFailed>());
}
#endif
