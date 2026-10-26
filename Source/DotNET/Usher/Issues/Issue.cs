// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.Conversation.AnswerQuestion;
using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Lifecycle.ReopenIssue;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;
using Cratis.Showrunner.Usher.Receiving;
using Cratis.Showrunner.Usher.Reporting.ReportError;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;

namespace Cratis.Showrunner.Usher.Issues;

/// <summary>
/// The state the reporter's commands decide on, one per issue. It is the write side's own model, never shown to
/// anyone: the reporter's views are <c>ReportedIssue</c> and <c>IssueConversation</c>.
/// </summary>
/// <remarks>
/// It is a self-reducing read model because the open questions are a set the rules check membership of. With read
/// protection on, a command's validator reads it fresh from the event log and the read guards the append.
/// Once withdrawn, an issue keeps its state: a desk event that crossed the withdrawal does not reopen it.
/// </remarks>
/// <param name="Id">The issue.</param>
/// <param name="ReportedBy">Who reported the issue, at their identity provider.</param>
/// <param name="State">The state the issue is in.</param>
/// <param name="IdentityShared">Whether the reporter let support know who they are.</param>
/// <param name="OpenQuestions">The questions support asked that the reporter has not answered.</param>
/// <param name="AccessGranted">Whether a grant of access is in effect, not revoked or lapsed; it may still have passed its end.</param>
/// <param name="AccessValidUntil">The end of the last grant; <see cref="Unset.ValidUntil"/> when there was none.</param>
public record Issue(
    IssueId Id,
    UserIdentity ReportedBy,
    IssueState State,
    bool IdentityShared,
    IEnumerable<QuestionId> OpenQuestions,
    bool AccessGranted,
    ValidUntil AccessValidUntil)
{
    /// <summary>
    /// Gets whether support can sign in as the reporter at a point in time.
    /// </summary>
    /// <param name="now">The point in time.</param>
    /// <returns>True when a grant is in effect and has not passed its end.</returns>
    public bool HasLiveGrant(DateTimeOffset now) => AccessGranted && AccessValidUntil.Value > now;

    /// <summary>
    /// Opens the issue from a problem described by hand.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static Issue Reduce(ReporterDescribedIssue @event, Issue? current, EventContext context) =>
        current ?? Open(context, @event.ReportedBy);

    /// <summary>
    /// Opens the issue from a reported error.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static Issue Reduce(ReporterFlaggedError @event, Issue? current, EventContext context) =>
        current ?? Open(context, @event.ReportedBy);

    /// <summary>
    /// Records that the reporter shared who they are.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Issue? Reduce(ReporterSharedIdentity @event, Issue? current) =>
        current is null ? null : current with { IdentityShared = true };

    /// <summary>
    /// Returns a reopened issue to reported.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Issue? Reduce(ReporterReopenedIssue @event, Issue? current) => Move(current, IssueState.Reported);

    /// <summary>
    /// Withdraws the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Issue? Reduce(ReporterWithdrewIssue @event, Issue? current) => Move(current, IssueState.Withdrawn);

    /// <summary>
    /// Records a grant of access.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Issue? Reduce(ReporterGrantedAccess @event, Issue? current) =>
        current is null ? null : current with { AccessGranted = true, AccessValidUntil = @event.ValidUntil };

    /// <summary>
    /// Ends the grant of access.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Issue? Reduce(ReporterRevokedAccess @event, Issue? current) =>
        current is null ? null : current with { AccessGranted = false };

    /// <summary>
    /// Ends the grant of access when the grant that lapsed is the current one.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Issue? Reduce(AccessGrantLapsed @event, Issue? current) =>
        current is null || current.AccessValidUntil != @event.ValidUntil ? current : current with { AccessGranted = false };

    /// <summary>
    /// Records that the desk triaged the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Issue? Reduce(SupportTriagedIssue @event, Issue? current) => Move(current, IssueState.Triaged);

    /// <summary>
    /// Records that the desk accepted the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Issue? Reduce(SupportAcceptedIssue @event, Issue? current) => Move(current, IssueState.Accepted);

    /// <summary>
    /// Records that the desk will not fix the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Issue? Reduce(SupportDeclinedToFix @event, Issue? current) => Move(current, IssueState.WontFix);

    /// <summary>
    /// Records that the desk started work on the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Issue? Reduce(SupportStartedWork @event, Issue? current) => Move(current, IssueState.InProgress);

    /// <summary>
    /// Records that the desk resolved the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Issue? Reduce(SupportResolvedIssue @event, Issue? current) => Move(current, IssueState.Resolved);

    /// <summary>
    /// Opens a question.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Issue? Reduce(SupportAskedQuestion @event, Issue? current) =>
        current?.OpenQuestions.Contains(@event.QuestionId) != false
            ? current
            : current with { OpenQuestions = [.. current.OpenQuestions, @event.QuestionId] };

    /// <summary>
    /// Closes an answered question.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Issue? Reduce(ReporterAnsweredQuestion @event, Issue? current) =>
        current is null ? null : current with { OpenQuestions = [.. current.OpenQuestions.Where(_ => _ != @event.QuestionId)] };

    static Issue Open(EventContext context, UserIdentity reportedBy) => new(
        new IssueId(Guid.Parse(context.EventSourceId.Value)),
        reportedBy,
        IssueState.Reported,
        false,
        [],
        false,
        Unset.ValidUntil);

    static Issue? Move(Issue? current, IssueState state) =>
        current is null || current.State == IssueState.Withdrawn ? current : current with { State = state };
}
