// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Issues;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_support_signs_in_as_the_reporter : a_read_model<IssueConversation>
{
    readonly SupportSessionId _ended = "ended-session";
    readonly SupportSessionId _running = "running-session";
    IssueConversation _issue;

    async Task Because()
    {
        await Happened(
            IssueEvents.Described(Reporters.Identity),
            new ReporterGrantedAccess(Start.AddDays(1)),
            new SupportSignedInAsReporter(_ended, IssueEvents.Support),
            new SupportSignedInAsReporter(_ended, IssueEvents.Support),
            new SupportSessionEnded(_ended),
            new SupportSessionEnded(_ended),
            new SupportSignedInAsReporter(_running, IssueEvents.Agent));
        _issue = _scenario.Instance!;
    }

    [Fact]
    void should_have_each_session_once() => _issue.Access.Sessions.ShouldEqual(
    [
        new SupportSession(_ended, IssueEvents.Support, At(2), true, At(4)),
        new SupportSession(_running, IssueEvents.Agent, At(6), false, Unset.Time)
    ]);

    [Fact]
    void should_record_each_sign_in_and_end_once_with_who() => _issue.Entries.Skip(2).Select(_ => (_.Kind, _.AuthorKind, (string)_.AuthorName)).ShouldEqual(
    [
        (ConversationEntryKind.SupportSignedIn, ConversationAuthorKind.Support, "Grace"),
        (ConversationEntryKind.SupportSessionEnded, ConversationAuthorKind.Support, "Grace"),
        (ConversationEntryKind.SupportSignedIn, ConversationAuthorKind.Agent, "Usher Agent")
    ]);
}
#endif
