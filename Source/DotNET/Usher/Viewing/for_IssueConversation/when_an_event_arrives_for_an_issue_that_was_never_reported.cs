// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_an_event_arrives_for_an_issue_that_was_never_reported : a_read_model<IssueConversation>
{
    IssueConversation? _issue;

    async Task Because()
    {
        await Happened(new SupportReplied("Hello?", IssueEvents.Support));
        _issue = _scenario.Instance;
    }

    [Fact] void should_not_have_a_conversation() => _issue.ShouldBeNull();
}
#endif
