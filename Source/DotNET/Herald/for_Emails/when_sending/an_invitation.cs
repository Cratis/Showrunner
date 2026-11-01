// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;

namespace Cratis.Showrunner.Herald.for_Emails.when_sending;

public class an_invitation : given.an_emails_service
{
    EmailId _id;
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        _id = await _emails.Send(new EmailRequest("invitation", Invitation, "Grace <grace@example.com>", InvitationValues) { Language = "nb-NO" });
        _facts = await Facts();
    }

    [Fact] void should_queue_the_email_against_what_it_is_about() => Assert.IsType<EmailQueued>(Assert.Single(_facts));
    [Fact] void should_queue_it_with_the_id_it_returns() => Assert.Equal(_id, ((EmailQueued)_facts[0]).EmailId);
    [Fact] void should_queue_the_bare_address() => Assert.Equal("grace@example.com", ((EmailQueued)_facts[0]).Address.Value);
    [Fact] void should_queue_the_language_asked_for() => Assert.Equal("nb-NO", ((EmailQueued)_facts[0]).Language.Value);
    [Fact] void should_queue_the_values() => Assert.Equal("Ada", ((EmailQueued)_facts[0]).Values.ToDictionary()["inviter.name"]);
}
#endif
