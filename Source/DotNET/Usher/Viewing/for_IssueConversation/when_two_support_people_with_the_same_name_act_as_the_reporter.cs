// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Conversation.AddDetails;
using Cratis.Showrunner.Usher.given;
using ChronicleIdentity = Cratis.Chronicle.Identities.Identity;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

/// <summary>
/// Two support people are both called Grace. Only one of them has a session on the issue: what the other does as the
/// reporter is never credited to that session.
/// </summary>
public class when_two_support_people_with_the_same_name_act_as_the_reporter : a_read_model<IssueConversation>
{
    static readonly ChronicleIdentity _reporter = new((string)Reporters.Subject, "Ada Lovelace", "ada@example.com");
    static readonly ChronicleIdentity _grace = new("support-person", "Grace Hopper", "Grace", _reporter);
    static readonly ChronicleIdentity _anotherGrace = new("another-support-person", "Grace Kelly", "Grace", _reporter);
    readonly SupportSessionId _graces = "graces-session";
    IssueConversation _issue;

    async Task Because()
    {
        await HappenedCausedBy(_reporter, IssueEvents.Described(Reporters.Identity), new ReporterGrantedAccess(Start.AddDays(1)));
        await HappenedCausedBy(_grace, new SupportSignedInAsReporter(_graces, IssueEvents.Support, new("entra", _grace.Subject)));
        await HappenedCausedBy(_anotherGrace, new ReporterAddedDetails("Somebody else's note."));
        await HappenedCausedBy(_grace, new ReporterAddedDetails("Clicked send twice."));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_not_credit_the_other_grace_to_the_session() => _issue.Entries.SkipLast(1).Last().Session.ShouldEqual(SupportSessionId.NotSet);
    [Fact] void should_credit_grace_to_her_session() => _issue.Entries.Last().Session.ShouldEqual(_graces);
}
#endif
