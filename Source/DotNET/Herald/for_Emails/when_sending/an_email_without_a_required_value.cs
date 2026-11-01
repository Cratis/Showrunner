// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;
using Cratis.Showrunner.Herald.Sending;

namespace Cratis.Showrunner.Herald.for_Emails.when_sending;

public class an_email_without_a_required_value : given.an_emails_service
{
    IReadOnlyList<object> _facts;
    Exception _exception;

    async Task Because()
    {
        _exception = await Record.ExceptionAsync(() => _emails.Send(new EmailRequest("invitation", Invitation, "grace@example.com", Values(("inviter.name", "Ada")))));
        _facts = await Facts();
    }

    [Fact] void should_refuse_it() => Assert.IsType<EmailCannotBeRendered>(_exception);
    [Fact] void should_queue_nothing() => Assert.Empty(_facts);
}
#endif
