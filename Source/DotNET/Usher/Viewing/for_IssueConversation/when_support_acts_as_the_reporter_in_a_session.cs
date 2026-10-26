// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Conversation.AddDetails;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Lifecycle.ReopenIssue;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;
using ChronicleIdentity = Cratis.Chronicle.Identities.Identity;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_support_acts_as_the_reporter_in_a_session : a_read_model<IssueConversation>
{
    static readonly ChronicleIdentity _reporter = new((string)Reporters.Subject, "Ada Lovelace", "ada@example.com");
    static readonly ChronicleIdentity _grace = new("support-person", "Grace Hopper", "Grace", _reporter);
    static readonly ChronicleIdentity _agent = new("support-agent", "Agent", "Usher Agent", _reporter);
    static readonly SupportAuthor _agentAuthor = new("Usher Agent", SupportAuthorKind.Agent);
    readonly SupportSessionId _graces = "graces-session";
    readonly SupportSessionId _agents = "agents-session";
    IssueConversation _issue;

    async Task Because()
    {
        await HappenedCausedBy(_reporter, IssueEvents.Described(Reporters.Identity), new ReporterGrantedAccess(Start.AddDays(1)));
        await HappenedCausedBy(_grace, new SupportSignedInAsReporter(_graces, IssueEvents.Support));
        await HappenedCausedBy(_agent, new SupportSignedInAsReporter(_agents, _agentAuthor));
        await HappenedCausedBy(_grace, new ReporterAddedDetails("Clicked send twice."));
        await HappenedCausedBy(_agent, new ReporterAddedDetails("Tried again in another browser."));
        await HappenedCausedBy(_grace, new SupportSessionEnded(_graces));
        await HappenedCausedBy(_grace, new ReporterReopenedIssue("It happened again."));
        await HappenedCausedBy(_agent, new SupportSessionEnded(_agents));
        await HappenedCausedBy(_reporter, new ReporterAddedDetails("Thanks."));
        _issue = _scenario.Instance!;
    }

    [Fact]
    void should_credit_each_entry_to_who_acted_and_in_which_session() => _issue.Entries.Select(_ => (_.Kind, (string)_.ActedBy, (string)_.Session)).ShouldEqual(
    [
        (ConversationEntryKind.IssueDescribed, string.Empty, string.Empty),
        (ConversationEntryKind.AccessGranted, string.Empty, string.Empty),
        (ConversationEntryKind.SupportSignedIn, "Grace", "graces-session"),
        (ConversationEntryKind.SupportSignedIn, "Usher Agent", "agents-session"),
        (ConversationEntryKind.DetailsAdded, "Grace", "graces-session"),
        (ConversationEntryKind.DetailsAdded, "Usher Agent", "agents-session"),
        (ConversationEntryKind.SupportSessionEnded, "Grace", "graces-session"),

        // Grace's session ended: what she still does as the reporter names her, but no session.
        (ConversationEntryKind.Reopened, "Grace", string.Empty),
        (ConversationEntryKind.SupportSessionEnded, "Usher Agent", "agents-session"),
        (ConversationEntryKind.DetailsAdded, string.Empty, string.Empty)
    ]);
}
#endif
