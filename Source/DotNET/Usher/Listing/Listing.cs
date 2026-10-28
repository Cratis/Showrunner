// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.KnownIssues;
using Cratis.Showrunner.Usher.Contracts.WorkItems;
using Cratis.Showrunner.Usher.Conversation.AddDetails;
using Cratis.Showrunner.Usher.Conversation.AnswerQuestion;
using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Issues;
using Cratis.Showrunner.Usher.Lifecycle.ReopenIssue;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;
using Cratis.Showrunner.Usher.Receiving;
using Cratis.Showrunner.Usher.Reporting.ReportError;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;

namespace Cratis.Showrunner.Usher.Listing;

/// <summary>
/// One of the reporter's issues, as the list of their issues shows it.
/// </summary>
/// <remarks>
/// It is a self-reducing read model: whether support waits for the reporter depends on which questions are still
/// open, and every fact on the issue moves its last activity. Once withdrawn, the state stays withdrawn; a desk event
/// that crossed the withdrawal does not change it. Reopening clears the release with the fix, because the fix did not
/// fix it.
/// </remarks>
/// <param name="Id">The issue.</param>
/// <param name="Kind">How the issue was reported.</param>
/// <param name="Title">The summary of a problem described by hand, or the message of a reported error.</param>
/// <param name="State">The state the issue is in.</param>
/// <param name="ReportedAt">When the issue was reported.</param>
/// <param name="LastActivityAt">When anything last happened on the issue.</param>
/// <param name="WaitingOnReporter">Whether support asked something the reporter has not answered.</param>
/// <param name="KnownIssueTitle">The title of the known issue it is linked to; <see cref="KnownIssueTitle.NotSet"/> when it is not.</param>
/// <param name="WorkItem">The work item it is linked to; <see cref="Unset.WorkItem"/> when it is not.</param>
/// <param name="FixedInVersion">The version that has the fix; <see cref="ReleaseVersion.NotSet"/> until a release ships it.</param>
/// <param name="ReportedBy">Who reported the issue, at their identity provider.</param>
/// <param name="OpenQuestions">The questions support asked that the reporter has not answered.</param>
[ReadModel]
public record ReportedIssue(
    IssueId Id,
    IssueKind Kind,
    IssueTitle Title,
    IssueState State,
    DateTimeOffset ReportedAt,
    DateTimeOffset LastActivityAt,
    bool WaitingOnReporter,
    KnownIssueTitle KnownIssueTitle,
    WorkItem WorkItem,
    ReleaseVersion FixedInVersion,
    UserIdentity ReportedBy,
    IEnumerable<QuestionId> OpenQuestions)
{
    /// <summary>
    /// Observes the issues the signed-in reporter reported, in the current tenant, most recent activity first.
    /// </summary>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current tenant's event store namespace.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    /// <returns>The reporter's issues, again every time one changes.</returns>
    [Authorize]
    public static ISubject<IEnumerable<ReportedIssue>> MyIssues(IReadModels readModels, ISignedInReporter reporter)
    {
        if (!reporter.IsSignedIn)
        {
            return new BehaviorSubject<IEnumerable<ReportedIssue>>([]);
        }

        var identity = reporter.Identity;
        if (reporter.IsSupportActingAsReporter)
        {
            // Support signed in as the reporter sees only the issue its session is for, and only while it is live.
            if (!reporter.TryGetLiveSupportSession(out var session))
            {
                return new BehaviorSubject<IEnumerable<ReportedIssue>>([]);
            }

            var sessionIssue = session.IssueId;
            return readModels.Observe<ReportedIssue>(issue => issue.ReportedBy == identity && issue.Id == sessionIssue);
        }

        return ObservedQueries.Map(
            readModels.Observe<ReportedIssue>(issue => issue.ReportedBy == identity),
            issues => (IEnumerable<ReportedIssue>)[.. issues.OrderByDescending(issue => issue.LastActivityAt)]);
    }

    /// <summary>
    /// Opens the issue from a problem described by hand.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue Reduce(ReporterDescribedIssue @event, ReportedIssue? current, EventContext context) =>
        current ?? ReportedIssueTransitions.Open(context, IssueKind.Manual, (string)@event.Summary, @event.ReportedBy);

    /// <summary>
    /// Opens the issue from a reported error.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue Reduce(ReporterFlaggedError @event, ReportedIssue? current, EventContext context) =>
        current ?? ReportedIssueTransitions.Open(context, IssueKind.Error, (string)@event.SeenMessage, @event.ReportedBy);

    /// <summary>
    /// Records activity when the reporter shares who they are.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(ReporterSharedIdentity @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context);

    /// <summary>
    /// Records activity when the reporter adds details.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(ReporterAddedDetails @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context);

    /// <summary>
    /// Closes the answered question.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(ReporterAnsweredQuestion @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context, issue => issue with { OpenQuestions = [.. issue.OpenQuestions.Where(_ => _ != @event.QuestionId)] });

    /// <summary>
    /// Returns the issue to reported and clears the release that did not fix it.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(ReporterReopenedIssue @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context, issue => ReportedIssueTransitions.Move(issue, IssueState.Reported) with { FixedInVersion = ReleaseVersion.NotSet });

    /// <summary>
    /// Withdraws the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(ReporterWithdrewIssue @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context, issue => ReportedIssueTransitions.Move(issue, IssueState.Withdrawn));

    /// <summary>
    /// Records activity when the reporter grants access.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(ReporterGrantedAccess @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context);

    /// <summary>
    /// Records activity when the reporter revokes access.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(ReporterRevokedAccess @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context);

    /// <summary>
    /// Records activity when a grant lapses.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(AccessGrantLapsed @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context);

    /// <summary>
    /// Records that the desk triaged the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(SupportTriagedIssue @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context, issue => ReportedIssueTransitions.Move(issue, IssueState.Triaged));

    /// <summary>
    /// Records that the desk accepted the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(SupportAcceptedIssue @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context, issue => ReportedIssueTransitions.Move(issue, IssueState.Accepted));

    /// <summary>
    /// Records that the desk will not fix the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(SupportDeclinedToFix @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context, issue => ReportedIssueTransitions.Move(issue, IssueState.WontFix));

    /// <summary>
    /// Opens the question support asked.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(SupportAskedQuestion @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context, issue => issue.OpenQuestions.Contains(@event.QuestionId)
            ? issue
            : issue with { OpenQuestions = [.. issue.OpenQuestions, @event.QuestionId] });

    /// <summary>
    /// Records activity when the desk replies.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(SupportReplied @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context);

    /// <summary>
    /// Links the issue to a known issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(SupportLinkedIssueToKnownIssue @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context, issue => issue with { KnownIssueTitle = @event.Title });

    /// <summary>
    /// Unlinks the issue from its known issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(SupportUnlinkedIssue @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context, issue => issue with { KnownIssueTitle = KnownIssueTitle.NotSet });

    /// <summary>
    /// Links the issue to a work item.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(SupportLinkedIssueToWorkItem @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context, issue => issue with { WorkItem = @event.WorkItem });

    /// <summary>
    /// Unlinks the issue from the work item, when it is the one it is linked to.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(SupportUnlinkedWorkItem @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context, issue => issue.WorkItem.Reference == @event.WorkItemReference ? issue with { WorkItem = Unset.WorkItem } : issue);

    /// <summary>
    /// Records that the desk started work on the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(SupportStartedWork @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context, issue => ReportedIssueTransitions.Move(issue, IssueState.InProgress));

    /// <summary>
    /// Records that the desk resolved the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(SupportResolvedIssue @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context, issue => ReportedIssueTransitions.Move(issue, IssueState.Resolved));

    /// <summary>
    /// Records activity when the desk requests access.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(SupportRequestedAccess @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context);

    /// <summary>
    /// Records activity when support signs in as the reporter.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(SupportSignedInAsReporter @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context);

    /// <summary>
    /// Records activity when a support session ends.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(SupportSessionEnded @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context);

    /// <summary>
    /// Records the version that has the fix.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ReportedIssue? Reduce(ReleaseShippedFix @event, ReportedIssue? current, EventContext context) =>
        ReportedIssueTransitions.Touch(current, context, issue => issue with { FixedInVersion = @event.ReleaseVersion });
}

/// <summary>
/// The transitions <see cref="ReportedIssue"/> shares between its reduce methods. They live outside the read model because
/// Arc treats every static method on a read model as a query.
/// </summary>
internal static class ReportedIssueTransitions
{
    /// <summary>
    /// Opens the issue as reported.
    /// </summary>
    /// <param name="context">The context of the event that opened it.</param>
    /// <param name="kind">How it was reported.</param>
    /// <param name="title">What it is shown as.</param>
    /// <param name="reportedBy">Who reported it.</param>
    /// <returns>The opened issue.</returns>
    internal static ReportedIssue Open(EventContext context, IssueKind kind, IssueTitle title, UserIdentity reportedBy) => new(
        new IssueId(Guid.Parse(context.EventSourceId.Value)),
        kind,
        title,
        IssueState.Reported,
        context.Occurred,
        context.Occurred,
        false,
        KnownIssueTitle.NotSet,
        Unset.WorkItem,
        ReleaseVersion.NotSet,
        reportedBy,
        []);

    /// <summary>
    /// Applies a change to an existing issue and records the activity; an issue that does not exist stays absent.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <param name="change">The change, if the event changes more than the last activity.</param>
    /// <returns>The next state.</returns>
    internal static ReportedIssue? Touch(ReportedIssue? current, EventContext context, Func<ReportedIssue, ReportedIssue>? change = null)
    {
        if (current is null)
        {
            return null;
        }

        var next = (change ?? (issue => issue))(current) with { LastActivityAt = context.Occurred };
        return next with { WaitingOnReporter = next.State != IssueState.Withdrawn && next.OpenQuestions.Any() };
    }

    /// <summary>
    /// Moves the issue to a state, unless it is withdrawn.
    /// </summary>
    /// <param name="issue">The issue.</param>
    /// <param name="state">The state to move to.</param>
    /// <returns>The next state.</returns>
    internal static ReportedIssue Move(ReportedIssue issue, IssueState state) =>
        issue.State == IssueState.Withdrawn ? issue : issue with { State = state };
}
