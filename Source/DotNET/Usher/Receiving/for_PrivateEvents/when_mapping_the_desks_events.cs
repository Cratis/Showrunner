// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.KnownIssues;
using Cratis.Showrunner.Usher.Contracts.Lifecycle;
using Cratis.Showrunner.Usher.Contracts.WorkItems;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Receiving.for_PrivateEvents;

public class when_mapping_the_desks_events : Specification
{
    static readonly TenantId _tenant = "fjord-logistics";
    static readonly QuestionId _questionId = QuestionId.New();
    static readonly KnownIssueId _knownIssueId = new(Guid.NewGuid());
    static readonly WorkItem _workItem = new("GitHub", "Cratis/Ledger#42", "Invoices are not sent", "https://github.com/Cratis/Ledger/issues/42");

    static (TenantId TenantId, object Event)? Map(object deskEvent) => PrivateEvents.For(deskEvent);

    [Fact] void should_map_issue_triaged_to_support_triaged_issue() =>
        Map(new IssueTriaged(_tenant)).ShouldEqual((_tenant, new SupportTriagedIssue()));

    [Fact] void should_map_issue_accepted_to_support_accepted_issue() =>
        Map(new IssueAccepted(_tenant)).ShouldEqual((_tenant, new SupportAcceptedIssue()));

    [Fact] void should_map_issue_closed_as_wont_fix_to_support_declined_to_fix() =>
        Map(new IssueClosedAsWontFix(_tenant, "Works as designed.")).ShouldEqual((_tenant, new SupportDeclinedToFix("Works as designed.")));

    [Fact] void should_map_question_asked_to_support_asked_question() =>
        Map(new QuestionAsked(_tenant, _questionId, "Which browser?", IssueEvents.Support))
            .ShouldEqual((_tenant, new SupportAskedQuestion(_questionId, "Which browser?", IssueEvents.Support)));

    [Fact] void should_map_reply_sent_to_support_replied() =>
        Map(new ReplySent(_tenant, "We are on it.", IssueEvents.Agent)).ShouldEqual((_tenant, new SupportReplied("We are on it.", IssueEvents.Agent)));

    [Fact] void should_map_issue_linked_to_known_issue_to_support_linked_issue_to_known_issue() =>
        Map(new IssueLinkedToKnownIssue(_tenant, _knownIssueId, "Invoices stuck"))
            .ShouldEqual((_tenant, new SupportLinkedIssueToKnownIssue(_knownIssueId, "Invoices stuck")));

    [Fact] void should_map_issue_unlinked_from_known_issue_to_support_unlinked_issue() =>
        Map(new IssueUnlinkedFromKnownIssue(_tenant, _knownIssueId)).ShouldEqual((_tenant, new SupportUnlinkedIssue(_knownIssueId)));

    [Fact] void should_map_issue_linked_to_work_item_to_support_linked_issue_to_work_item() =>
        Map(new IssueLinkedToWorkItem(_tenant, _workItem)).ShouldEqual((_tenant, new SupportLinkedIssueToWorkItem(_workItem)));

    [Fact] void should_map_issue_unlinked_from_work_item_to_support_unlinked_work_item() =>
        Map(new IssueUnlinkedFromWorkItem(_tenant, _workItem.Reference)).ShouldEqual((_tenant, new SupportUnlinkedWorkItem(_workItem.Reference)));

    [Fact] void should_map_work_started_to_support_started_work() =>
        Map(new WorkStarted(_tenant)).ShouldEqual((_tenant, new SupportStartedWork()));

    [Fact] void should_map_issue_resolved_to_support_resolved_issue() =>
        Map(new IssueResolved(_tenant, "Fixed in 4.12.3.")).ShouldEqual((_tenant, new SupportResolvedIssue("Fixed in 4.12.3.")));

    [Fact] void should_map_issue_fixed_in_release_to_release_shipped_fix() =>
        Map(new IssueFixedInRelease(_tenant, "4.12.3", _workItem.Reference)).ShouldEqual((_tenant, new ReleaseShippedFix("4.12.3", _workItem.Reference)));

    [Fact] void should_map_access_requested_to_support_requested_access() =>
        Map(new AccessRequested(_tenant, "To reproduce the problem.")).ShouldEqual((_tenant, new SupportRequestedAccess("To reproduce the problem.")));

    [Fact] void should_not_map_an_event_the_desk_does_not_send() => Map(new IssueWithdrawn(_tenant)).ShouldBeNull();
}
#endif
