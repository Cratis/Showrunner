// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;

namespace Cratis.Showrunner.Herald.for_Emails.when_recording_a_bounce;

public class an_email : given.an_emails_service
{
    EmailId _id = EmailId.New();
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        await _emails.Bounced(Invitation, _id, "invitation", "Mailbox full");
        _facts = await Facts();
    }

    [Fact] void should_record_the_bounce_against_what_it_is_about() => Assert.Equal(new EmailBounced(_id, "invitation", "Mailbox full"), Assert.Single(_facts));
}
#endif
