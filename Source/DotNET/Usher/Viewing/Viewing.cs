// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.Contracts.KnownIssues;
using Cratis.Showrunner.Usher.Contracts.Reporting;
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

namespace Cratis.Showrunner.Usher.Viewing;

/// <summary>
/// What an entry in the conversation on an issue records.
/// </summary>
public enum ConversationEntryKind
{
    /// <summary>The reporter reported an error; the text is its message.</summary>
    ErrorReported = 0,

    /// <summary>The reporter described a problem; the text is the description.</summary>
    IssueDescribed = 1,

    /// <summary>The reporter added details; the text is what they added.</summary>
    DetailsAdded = 2,

    /// <summary>Support asked a question; the text is the question.</summary>
    QuestionAsked = 3,

    /// <summary>The reporter answered a question; the text is the answer.</summary>
    QuestionAnswered = 4,

    /// <summary>Support replied; the text is the reply.</summary>
    Replied = 5,

    /// <summary>The reporter let support know who they are.</summary>
    IdentityShared = 6,

    /// <summary>The reporter let support sign in as them.</summary>
    AccessGranted = 7,

    /// <summary>The reporter took back access.</summary>
    AccessRevoked = 8,

    /// <summary>A grant of access ended by its time passing.</summary>
    AccessLapsed = 9,

    /// <summary>Support asked for access; the text is the reason.</summary>
    AccessRequested = 10,

    /// <summary>Support signed in as the reporter.</summary>
    SupportSignedIn = 11,

    /// <summary>A session in which support signed in as the reporter ended.</summary>
    SupportSessionEnded = 12,

    /// <summary>The desk triaged the issue.</summary>
    Triaged = 13,

    /// <summary>The desk accepted the issue.</summary>
    Accepted = 14,

    /// <summary>The desk will not fix the issue; the text is the reason.</summary>
    DeclinedToFix = 15,

    /// <summary>The desk started work on the issue.</summary>
    WorkStarted = 16,

    /// <summary>The desk resolved the issue; the text is the resolution.</summary>
    Resolved = 17,

    /// <summary>The reporter reopened the issue; the text is the reason.</summary>
    Reopened = 18,

    /// <summary>The reporter withdrew the issue.</summary>
    Withdrawn = 19,

    /// <summary>The issue was linked to a known issue; the text is its title.</summary>
    LinkedToKnownIssue = 20,

    /// <summary>The issue was unlinked from its known issue.</summary>
    UnlinkedFromKnownIssue = 21,

    /// <summary>The issue was linked to a work item; the text is its title.</summary>
    LinkedToWorkItem = 22,

    /// <summary>The issue was unlinked from a work item; the text is its reference.</summary>
    UnlinkedFromWorkItem = 23,

    /// <summary>A release shipped the fix; the text is the version.</summary>
    FixReleased = 24
}

/// <summary>
/// Who an entry in the conversation is from.
/// </summary>
public enum ConversationAuthorKind
{
    /// <summary>The reporter.</summary>
    Reporter = 0,

    /// <summary>A person at the support desk.</summary>
    Support = 1,

    /// <summary>A support agent acting for the desk.</summary>
    Agent = 2,

    /// <summary>The desk or the application itself, for state changes, links and releases.</summary>
    System = 3
}

/// <summary>
/// Whether support may sign in as the reporter, as far as facts say. Whether a grant has passed its end is decided
/// against the clock where it is shown, from <see cref="IssueAccess.ValidUntil"/>.
/// </summary>
public enum AccessState
{
    /// <summary>The reporter never granted access.</summary>
    NotGranted = 0,

    /// <summary>The reporter granted access until <see cref="IssueAccess.ValidUntil"/>.</summary>
    Granted = 1,

    /// <summary>The reporter took the grant back.</summary>
    Revoked = 2,

    /// <summary>The grant ended by its time passing.</summary>
    Lapsed = 3
}

/// <summary>
/// Identifies an entry in the conversation on an issue: the sequence number of the event it records.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ConversationEntryId(ulong Value) : ConceptAs<ulong>(Value);

/// <summary>
/// The text of an entry in the conversation; empty for entries that only mark that something happened.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ConversationEntryText(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an entry without text.
    /// </summary>
    public static readonly ConversationEntryText NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ConversationEntryText"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ConversationEntryText(string value) => new(value);
}

/// <summary>
/// One entry in the conversation on an issue.
/// </summary>
/// <param name="EntryId">The entry.</param>
/// <param name="Kind">What the entry records.</param>
/// <param name="Text">The text of the entry; empty when it only marks that something happened.</param>
/// <param name="AuthorKind">Who the entry is from.</param>
/// <param name="AuthorName">The name of the support person or agent, <see cref="SupportAuthorName.NotSet"/> when the desk hides it or the entry is not from support.</param>
/// <param name="At">When it happened.</param>
/// <param name="QuestionId">The question a question or an answer is about; <see cref="Unset.QuestionId"/> for other entries.</param>
public record ConversationEntry(
    ConversationEntryId EntryId,
    ConversationEntryKind Kind,
    ConversationEntryText Text,
    ConversationAuthorKind AuthorKind,
    SupportAuthorName AuthorName,
    DateTimeOffset At,
    QuestionId QuestionId);

/// <summary>
/// The question support waits for the reporter to answer.
/// </summary>
/// <param name="QuestionId">The question; <see cref="Unset.QuestionId"/> when there is none.</param>
/// <param name="Question">What support asked.</param>
public record OpenQuestion(QuestionId QuestionId, QuestionText Question)
{
    /// <summary>
    /// No open question.
    /// </summary>
    public static readonly OpenQuestion None = new(Unset.QuestionId, QuestionText.NotSet);
}

/// <summary>
/// Support's request for access the reporter has not answered.
/// </summary>
/// <param name="Pending">Whether support waits for the reporter to grant access.</param>
/// <param name="Reason">Why support asked.</param>
public record AccessRequest(bool Pending, AccessRequestReason Reason)
{
    /// <summary>
    /// No pending request.
    /// </summary>
    public static readonly AccessRequest None = new(false, AccessRequestReason.NotSet);
}

/// <summary>
/// A session in which support signed in as the reporter.
/// </summary>
/// <param name="SessionId">The session.</param>
/// <param name="Author">Who signed in.</param>
/// <param name="StartedAt">When they signed in.</param>
/// <param name="Ended">Whether the session ended.</param>
/// <param name="EndedAt">When it ended; <see cref="Unset.Time"/> while it runs.</param>
public record SupportSession(SupportSessionId SessionId, SupportAuthor Author, DateTimeOffset StartedAt, bool Ended, DateTimeOffset EndedAt);

/// <summary>
/// Support's access to sign in as the reporter on an issue.
/// </summary>
/// <param name="State">Whether access is granted.</param>
/// <param name="ValidUntil">The end of the last grant; <see cref="Unset.ValidUntil"/> when there was none.</param>
/// <param name="Request">Support's pending request for access.</param>
/// <param name="Sessions">The sessions in which support signed in as the reporter, oldest first.</param>
public record IssueAccess(AccessState State, ValidUntil ValidUntil, AccessRequest Request, IEnumerable<SupportSession> Sessions)
{
    /// <summary>
    /// Access never granted or requested.
    /// </summary>
    public static readonly IssueAccess None = new(AccessState.NotGranted, Unset.ValidUntil, AccessRequest.None, []);
}

/// <summary>
/// One of the reporter's issues with everything that happened on it, as the issue view shows it.
/// </summary>
/// <remarks>
/// It is a self-reducing read model: every fact appends an entry, and the open question follows which questions have
/// been answered. Once withdrawn, the state stays withdrawn. Reopening clears the release with the fix, because the
/// fix did not fix it; the entry for that release stays in the conversation.
/// </remarks>
/// <param name="Id">The issue.</param>
/// <param name="Title">The summary of a problem described by hand, or the message of a reported error.</param>
/// <param name="State">The state the issue is in.</param>
/// <param name="Location">Where in the application the issue was reported.</param>
/// <param name="ReportedAt">When the issue was reported.</param>
/// <param name="IdentityShared">Whether the reporter let support know who they are.</param>
/// <param name="OpenQuestion">The most recent question the reporter has not answered; <see cref="OpenQuestion.None"/> when there is none.</param>
/// <param name="Entries">Everything that happened on the issue, oldest first.</param>
/// <param name="Access">Support's access to sign in as the reporter.</param>
/// <param name="KnownIssueTitle">The title of the known issue it is linked to; <see cref="KnownIssueTitle.NotSet"/> when it is not.</param>
/// <param name="WorkItem">The work item it is linked to; <see cref="Unset.WorkItem"/> when it is not.</param>
/// <param name="FixedInVersion">The version that has the fix; <see cref="ReleaseVersion.NotSet"/> until a release ships it.</param>
/// <param name="ReporterSubject">The subject of the reporter who reported the issue.</param>
[ReadModel]
public record IssueConversation(
    IssueId Id,
    IssueTitle Title,
    IssueState State,
    SliceLocation Location,
    DateTimeOffset ReportedAt,
    bool IdentityShared,
    OpenQuestion OpenQuestion,
    IEnumerable<ConversationEntry> Entries,
    IssueAccess Access,
    KnownIssueTitle KnownIssueTitle,
    WorkItem WorkItem,
    ReleaseVersion FixedInVersion,
    IdentityProviderSubject ReporterSubject)
{
    /// <summary>
    /// Observes one issue the signed-in reporter reported, in the current tenant. An issue someone else reported is
    /// never emitted; like an issue that does not exist, it is emitted as nothing.
    /// </summary>
    /// <param name="issueId">The issue.</param>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current tenant's event store namespace.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    /// <returns>The issue, again every time it changes.</returns>
    [Authorize]
    public static ISubject<IssueConversation> ForIssue(IssueId issueId, IReadModels readModels, ISignedInReporter reporter)
    {
        if (!reporter.IsSignedIn)
        {
            return new BehaviorSubject<IssueConversation>(null!);
        }

        var subject = reporter.Subject;
        return ObservedQueries.Map(
            readModels.Observe<IssueConversation>(issue => issue.Id == issueId && issue.ReporterSubject == subject),
            issues => issues.FirstOrDefault()!);
    }

    /// <summary>
    /// Opens the conversation with a problem described by hand.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation Reduce(ReporterDescribedIssue @event, IssueConversation? current, EventContext context) =>
        current ?? IssueConversationTransitions.Open(context, (string)@event.Summary, @event.Location, @event.ReporterSubject, ConversationEntryKind.IssueDescribed, (string)@event.Description);

    /// <summary>
    /// Opens the conversation with a reported error.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation Reduce(ReporterFlaggedError @event, IssueConversation? current, EventContext context) =>
        current ?? IssueConversationTransitions.Open(context, (string)@event.ErrorMessage, @event.Location, @event.ReporterSubject, ConversationEntryKind.ErrorReported, (string)@event.ErrorMessage);

    /// <summary>
    /// Records that the reporter shared who they are.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(ReporterSharedIdentity @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.ByReporter(context, ConversationEntryKind.IdentityShared), issue => issue with { IdentityShared = true });

    /// <summary>
    /// Adds the details the reporter added.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(ReporterAddedDetails @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.ByReporter(context, ConversationEntryKind.DetailsAdded, (string)@event.Details));

    /// <summary>
    /// Adds the reporter's answer and moves the open question to the next unanswered one.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(ReporterAnsweredQuestion @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.ByReporter(context, ConversationEntryKind.QuestionAnswered, (string)@event.Answer) with { QuestionId = @event.QuestionId });

    /// <summary>
    /// Returns the issue to reported and clears the release that did not fix it.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(ReporterReopenedIssue @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.ByReporter(context, ConversationEntryKind.Reopened, (string)@event.Reason), issue => IssueConversationTransitions.Move(issue, IssueState.Reported) with { FixedInVersion = ReleaseVersion.NotSet });

    /// <summary>
    /// Withdraws the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(ReporterWithdrewIssue @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.ByReporter(context, ConversationEntryKind.Withdrawn), issue => IssueConversationTransitions.Move(issue, IssueState.Withdrawn));

    /// <summary>
    /// Records the grant of access and answers support's request for it.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(ReporterGrantedAccess @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.ByReporter(context, ConversationEntryKind.AccessGranted), issue => issue with
        {
            Access = issue.Access with { State = AccessState.Granted, ValidUntil = @event.ValidUntil, Request = AccessRequest.None }
        });

    /// <summary>
    /// Records that the reporter took access back.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(ReporterRevokedAccess @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.ByReporter(context, ConversationEntryKind.AccessRevoked), issue => issue with
        {
            Access = issue.Access with { State = AccessState.Revoked }
        });

    /// <summary>
    /// Records that the grant ended by its time passing, when the grant that lapsed is the current one.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(AccessGrantLapsed @event, IssueConversation? current, EventContext context) =>
        current is null || current.Access.State != AccessState.Granted || current.Access.ValidUntil != @event.ValidUntil
            ? current
            : IssueConversationTransitions.Add(current, IssueConversationTransitions.BySystem(context, ConversationEntryKind.AccessLapsed), issue => issue with
            {
                Access = issue.Access with { State = AccessState.Lapsed }
            });

    /// <summary>
    /// Records that the desk triaged the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportTriagedIssue @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.BySystem(context, ConversationEntryKind.Triaged), issue => IssueConversationTransitions.Move(issue, IssueState.Triaged));

    /// <summary>
    /// Records that the desk accepted the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportAcceptedIssue @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.BySystem(context, ConversationEntryKind.Accepted), issue => IssueConversationTransitions.Move(issue, IssueState.Accepted));

    /// <summary>
    /// Records that the desk will not fix the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportDeclinedToFix @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.BySystem(context, ConversationEntryKind.DeclinedToFix, (string)@event.Reason), issue => IssueConversationTransitions.Move(issue, IssueState.WontFix));

    /// <summary>
    /// Adds the question support asked.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportAskedQuestion @event, IssueConversation? current, EventContext context) =>
        current?.Entries.Any(entry => entry.Kind == ConversationEntryKind.QuestionAsked && entry.QuestionId == @event.QuestionId) != false
            ? current
            : IssueConversationTransitions.Add(current, IssueConversationTransitions.BySupport(context, ConversationEntryKind.QuestionAsked, (string)@event.Question, @event.Author) with { QuestionId = @event.QuestionId });

    /// <summary>
    /// Adds the reply support sent.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportReplied @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.BySupport(context, ConversationEntryKind.Replied, (string)@event.Reply, @event.Author));

    /// <summary>
    /// Links the issue to a known issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportLinkedIssueToKnownIssue @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.BySystem(context, ConversationEntryKind.LinkedToKnownIssue, (string)@event.Title), issue => issue with { KnownIssueTitle = @event.Title });

    /// <summary>
    /// Unlinks the issue from its known issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportUnlinkedIssue @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.BySystem(context, ConversationEntryKind.UnlinkedFromKnownIssue), issue => issue with { KnownIssueTitle = KnownIssueTitle.NotSet });

    /// <summary>
    /// Links the issue to a work item.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportLinkedIssueToWorkItem @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.BySystem(context, ConversationEntryKind.LinkedToWorkItem, (string)@event.WorkItem.Title), issue => issue with { WorkItem = @event.WorkItem });

    /// <summary>
    /// Unlinks the issue from the work item, when it is the one it is linked to.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportUnlinkedWorkItem @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.BySystem(context, ConversationEntryKind.UnlinkedFromWorkItem, (string)@event.WorkItemReference), issue =>
            issue.WorkItem.Reference == @event.WorkItemReference ? issue with { WorkItem = Unset.WorkItem } : issue);

    /// <summary>
    /// Records that the desk started work on the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportStartedWork @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.BySystem(context, ConversationEntryKind.WorkStarted), issue => IssueConversationTransitions.Move(issue, IssueState.InProgress));

    /// <summary>
    /// Records that the desk resolved the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportResolvedIssue @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.BySystem(context, ConversationEntryKind.Resolved, (string)@event.Resolution), issue => IssueConversationTransitions.Move(issue, IssueState.Resolved));

    /// <summary>
    /// Records support's request for access.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportRequestedAccess @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.BySystem(context, ConversationEntryKind.AccessRequested, (string)@event.Reason), issue => issue with
        {
            Access = issue.Access with { Request = new(true, @event.Reason) }
        });

    /// <summary>
    /// Starts a session in which support signed in as the reporter.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportSignedInAsReporter @event, IssueConversation? current, EventContext context) =>
        current?.Access.Sessions.Any(session => session.SessionId == @event.SessionId) != false
            ? current
            : IssueConversationTransitions.Add(current, IssueConversationTransitions.BySupport(context, ConversationEntryKind.SupportSignedIn, ConversationEntryText.NotSet, @event.Author), issue => issue with
            {
                Access = issue.Access with
                {
                    Sessions = [.. issue.Access.Sessions, new SupportSession(@event.SessionId, @event.Author, context.Occurred, false, Unset.Time)]
                }
            });

    /// <summary>
    /// Ends a session in which support signed in as the reporter.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportSessionEnded @event, IssueConversation? current, EventContext context)
    {
        var session = current?.Access.Sessions.FirstOrDefault(session => session.SessionId == @event.SessionId);
        if (session is not { Ended: false })
        {
            return current;
        }

        var entry = IssueConversationTransitions.BySupport(context, ConversationEntryKind.SupportSessionEnded, ConversationEntryText.NotSet, session.Author);
        return IssueConversationTransitions.Add(current, entry, issue => issue with
        {
            Access = issue.Access with
            {
                Sessions = [.. issue.Access.Sessions.Select(_ => _.SessionId == @event.SessionId ? _ with { Ended = true, EndedAt = context.Occurred } : _)]
            }
        });
    }

    /// <summary>
    /// Records the version that has the fix.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(ReleaseShippedFix @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, IssueConversationTransitions.BySystem(context, ConversationEntryKind.FixReleased, (string)@event.ReleaseVersion), issue => issue with { FixedInVersion = @event.ReleaseVersion });
}

/// <summary>
/// The transitions <see cref="IssueConversation"/> shares between its reduce methods. They live outside the read model because
/// Arc treats every static method on a read model as a query.
/// </summary>
internal static class IssueConversationTransitions
{
    /// <summary>
    /// Opens the conversation with the reporter's first entry.
    /// </summary>
    /// <param name="context">The context of the event that opened it.</param>
    /// <param name="title">What the issue is shown as.</param>
    /// <param name="location">Where it was reported.</param>
    /// <param name="reporterSubject">The subject of the reporter.</param>
    /// <param name="kind">What the first entry records.</param>
    /// <param name="text">The text of the first entry.</param>
    /// <returns>The opened conversation.</returns>
    internal static IssueConversation Open(
        EventContext context,
        IssueTitle title,
        SliceLocation location,
        IdentityProviderSubject reporterSubject,
        ConversationEntryKind kind,
        ConversationEntryText text) => new(
        new IssueId(Guid.Parse(context.EventSourceId.Value)),
        title,
        IssueState.Reported,
        location,
        context.Occurred,
        false,
        OpenQuestion.None,
        [ByReporter(context, kind, text)],
        IssueAccess.None,
        KnownIssueTitle.NotSet,
        Unset.WorkItem,
        ReleaseVersion.NotSet,
        reporterSubject);

    /// <summary>
    /// Adds an entry to an existing conversation, applies a change and finds the open question; a conversation that
    /// does not exist stays absent.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="entry">The entry to add.</param>
    /// <param name="change">The change, if the event changes more than the entries.</param>
    /// <returns>The next state.</returns>
    internal static IssueConversation? Add(IssueConversation? current, ConversationEntry entry, Func<IssueConversation, IssueConversation>? change = null)
    {
        if (current is null)
        {
            return null;
        }

        var next = (change ?? (issue => issue))(current) with { Entries = [.. current.Entries, entry] };
        return next with { OpenQuestion = OpenQuestionOf(next.Entries) };
    }

    /// <summary>
    /// Moves the issue to a state, unless it is withdrawn.
    /// </summary>
    /// <param name="issue">The issue.</param>
    /// <param name="state">The state to move to.</param>
    /// <returns>The next state.</returns>
    internal static IssueConversation Move(IssueConversation issue, IssueState state) =>
        issue.State == IssueState.Withdrawn ? issue : issue with { State = state };

    /// <summary>
    /// Creates an entry from the reporter.
    /// </summary>
    /// <param name="context">The context of the event.</param>
    /// <param name="kind">What the entry records.</param>
    /// <param name="text">The text, if any.</param>
    /// <returns>The entry.</returns>
    internal static ConversationEntry ByReporter(EventContext context, ConversationEntryKind kind, ConversationEntryText? text = null) =>
        Entry(context, kind, text ?? ConversationEntryText.NotSet, ConversationAuthorKind.Reporter, SupportAuthorName.NotSet);

    /// <summary>
    /// Creates an entry for a state change, link or release.
    /// </summary>
    /// <param name="context">The context of the event.</param>
    /// <param name="kind">What the entry records.</param>
    /// <param name="text">The text, if any.</param>
    /// <returns>The entry.</returns>
    internal static ConversationEntry BySystem(EventContext context, ConversationEntryKind kind, ConversationEntryText? text = null) =>
        Entry(context, kind, text ?? ConversationEntryText.NotSet, ConversationAuthorKind.System, SupportAuthorName.NotSet);

    /// <summary>
    /// Creates an entry from a support person or agent.
    /// </summary>
    /// <param name="context">The context of the event.</param>
    /// <param name="kind">What the entry records.</param>
    /// <param name="text">The text.</param>
    /// <param name="author">Who it is from.</param>
    /// <returns>The entry.</returns>
    internal static ConversationEntry BySupport(EventContext context, ConversationEntryKind kind, ConversationEntryText text, SupportAuthor author) =>
        Entry(
            context,
            kind,
            text,
            author.Kind == SupportAuthorKind.Agent ? ConversationAuthorKind.Agent : ConversationAuthorKind.Support,
            author.Name);

    static OpenQuestion OpenQuestionOf(IEnumerable<ConversationEntry> entries)
    {
        var answered = entries.Where(entry => entry.Kind == ConversationEntryKind.QuestionAnswered).Select(entry => entry.QuestionId).ToHashSet();
        var question = entries.LastOrDefault(entry => entry.Kind == ConversationEntryKind.QuestionAsked && !answered.Contains(entry.QuestionId));
        return question is null ? OpenQuestion.None : new(question.QuestionId, (string)question.Text);
    }

    static ConversationEntry Entry(
        EventContext context,
        ConversationEntryKind kind,
        ConversationEntryText text,
        ConversationAuthorKind authorKind,
        SupportAuthorName authorName) =>
        new(new(context.SequenceNumber.Value), kind, text, authorKind, authorName, context.Occurred, Unset.QuestionId);
}
