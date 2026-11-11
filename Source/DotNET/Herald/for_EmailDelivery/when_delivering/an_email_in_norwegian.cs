// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_in_norwegian : given.a_queued_email
{
    IReadOnlyList<object> _facts;
    EmailId _norwegian;

    async Task Because()
    {
        _norwegian = EmailId.New();
        await _eventStore.EventLog.Append(Invitation, new EmailQueued(_norwegian, "invitation", "nb", "grace@example.com", "Grace", EmailValues.From(InvitationValues), _time.GetUtcNow()));
        await _delivery.Deliver(_eventStore, Invitation, _norwegian);
        _facts = await Facts();
    }

    [Fact] void should_render_it_in_the_language_it_was_queued_with() => Assert.Equal("Ada har invitert deg til Ledger", _transport.Accepted.Single().Subject);
    [Fact] void should_record_the_language_it_was_sent_in() => Assert.Equal("nb", _facts.OfType<EmailSent>().Single(sent => sent.EmailId == _norwegian).Language.Value);
}
#endif
