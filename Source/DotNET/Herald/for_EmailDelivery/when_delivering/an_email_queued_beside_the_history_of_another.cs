// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_queued_beside_the_history_of_another : given.a_queued_email
{
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        var other = EmailId.New();
        await _eventStore.EventLog.Append(Invitation, new EmailQueued(other, "invitation", EmailLanguage.NotSet, "alan@example.com", "Alan", EmailValues.From(InvitationValues), _time.GetUtcNow()));
        await _eventStore.EventLog.Append(Invitation, new EmailAttemptFailed(other, 5, "down", _time.GetUtcNow() + TimeSpan.FromDays(1)));
        await _eventStore.EventLog.Append(Invitation, new EmailSent(other, "invitation", "en", 6));
        await _eventStore.EventLog.Append(Invitation, new EmailFailed(other, "invitation", "en", "refused", 6));
        await _delivery.Deliver(_eventStore, Invitation, _id);
        _facts = await Facts();
    }

    [Fact] void should_send_the_email_to_its_own_recipient() => Assert.Equal("grace@example.com", _transport.Accepted.Single().To.Address);
    [Fact] void should_count_only_its_own_attempts() => Assert.Equal(1, _facts.OfType<EmailSent>().Single(sent => sent.EmailId == _id).Attempts);
}
#endif
