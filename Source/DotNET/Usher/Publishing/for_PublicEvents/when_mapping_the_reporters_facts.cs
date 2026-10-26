// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.Contracts.Lifecycle;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Conversation.AddDetails;
using Cratis.Showrunner.Usher.Conversation.AnswerQuestion;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Lifecycle.ReopenIssue;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;
using Cratis.Showrunner.Usher.Reporting.ReportError;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;

namespace Cratis.Showrunner.Usher.Publishing.for_PublicEvents;

public class when_mapping_the_reporters_facts : Specification
{
    static readonly TenantId _tenant = "fjord-logistics";
    static readonly QuestionId _questionId = QuestionId.New();
    static readonly ValidUntil _validUntil = new(new DateTimeOffset(2026, 10, 27, 12, 0, 0, TimeSpan.Zero));

    ReporterFlaggedError _flagged;
    ReporterDescribedIssue _described;

    void Establish()
    {
        _flagged = IssueEvents.Flagged(Reporters.Identity);
        _described = IssueEvents.Described(Reporters.Identity);
    }

    [Fact] void should_map_a_flagged_error_to_error_reported() => PublicEvents.For(_flagged, _tenant).ShouldEqual(new ErrorReported(
        _tenant,
        _flagged.Location,
        _flagged.CorrelationIds,
        _flagged.StackFrames,
        _flagged.ErrorMessage,
        _flagged.ErrorType,
        _flagged.Fingerprint,
        _flagged.ApplicationName,
        _flagged.ApplicationVersion));

    [Fact] void should_map_a_described_issue_to_issue_reported() => PublicEvents.For(_described, _tenant).ShouldEqual(new IssueReported(
        _tenant,
        _described.Location,
        _described.CorrelationIds,
        _described.Summary,
        _described.Description,
        _described.ApplicationName,
        _described.ApplicationVersion));

    [Fact] void should_map_a_shared_identity_to_issue_attributed_to_reporter() =>
        PublicEvents.For(new ReporterSharedIdentity(Reporters.Reporter), _tenant).ShouldEqual(new IssueAttributedToReporter(_tenant, Reporters.Reporter));

    [Fact] void should_map_added_details_to_details_added() =>
        PublicEvents.For(new ReporterAddedDetails("It happens every time."), _tenant).ShouldEqual(new DetailsAdded(_tenant, "It happens every time."));

    [Fact] void should_map_an_answered_question_to_question_answered() =>
        PublicEvents.For(new ReporterAnsweredQuestion(_questionId, "Yes."), _tenant).ShouldEqual(new QuestionAnswered(_tenant, _questionId, "Yes."));

    [Fact] void should_map_a_reopened_issue_to_issue_reopened() =>
        PublicEvents.For(new ReporterReopenedIssue("It came back."), _tenant).ShouldEqual(new IssueReopened(_tenant, "It came back."));

    [Fact] void should_map_a_withdrawn_issue_to_issue_withdrawn() =>
        PublicEvents.For(new ReporterWithdrewIssue(), _tenant).ShouldEqual(new IssueWithdrawn(_tenant));

    [Fact] void should_map_granted_access_to_access_granted() =>
        PublicEvents.For(new ReporterGrantedAccess(_validUntil), _tenant).ShouldEqual(new AccessGranted(_tenant, _validUntil));

    [Fact] void should_map_revoked_access_to_access_revoked() =>
        PublicEvents.For(new ReporterRevokedAccess(), _tenant).ShouldEqual(new AccessRevoked(_tenant));

    [Fact] void should_map_a_lapsed_grant_to_access_expired() =>
        PublicEvents.For(new AccessGrantLapsed(_validUntil), _tenant).ShouldEqual(new AccessExpired(_tenant));
}
#endif
