// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.KnownIssues;
using Cratis.Showrunner.Usher.Contracts.WorkItems;
using Cratis.Showrunner.Usher.Issues;
using Cratis.Showrunner.Usher.Lifecycle.ReopenIssue;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Viewing;

/// <summary>
/// The reducers of <see cref="IssueConversation"/> for the life of the issue: reopening it, what the desk does with it, its links to known issues and work items, and the release that ships the fix.
/// </summary>
public partial record IssueConversation
{
    /// <summary>
    /// Returns the issue to reported and clears the release that did not fix it.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(ReporterReopenedIssue @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.ByReporter(context, ConversationEntryKind.Reopened, (string)@event.Reason), issue => IssueConversationTransitions.Move(issue, IssueState.Reported) with { FixedInVersion = ReleaseVersion.NotSet });

    /// <summary>
    /// Records that the desk triaged the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportTriagedIssue @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySystem(context, ConversationEntryKind.Triaged), issue => IssueConversationTransitions.Move(issue, IssueState.Triaged));

    /// <summary>
    /// Records that the desk accepted the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportAcceptedIssue @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySystem(context, ConversationEntryKind.Accepted), issue => IssueConversationTransitions.Move(issue, IssueState.Accepted));

    /// <summary>
    /// Records that the desk will not fix the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportDeclinedToFix @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySystem(context, ConversationEntryKind.DeclinedToFix, (string)@event.Reason), issue => IssueConversationTransitions.Move(issue, IssueState.WontFix));

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
            : IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySupport(context, ConversationEntryKind.QuestionAsked, (string)@event.Question, @event.Author) with { QuestionId = @event.QuestionId });

    /// <summary>
    /// Adds the reply support sent.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportReplied @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySupport(context, ConversationEntryKind.Replied, (string)@event.Reply, @event.Author));

    /// <summary>
    /// Links the issue to a known issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportLinkedIssueToKnownIssue @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySystem(context, ConversationEntryKind.LinkedToKnownIssue, (string)@event.Title), issue => issue with { KnownIssueTitle = @event.Title });

    /// <summary>
    /// Unlinks the issue from its known issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportUnlinkedIssue @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySystem(context, ConversationEntryKind.UnlinkedFromKnownIssue), issue => issue with { KnownIssueTitle = KnownIssueTitle.NotSet });

    /// <summary>
    /// Links the issue to a work item.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportLinkedIssueToWorkItem @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySystem(context, ConversationEntryKind.LinkedToWorkItem, (string)@event.WorkItem.Title), issue => issue with { WorkItem = @event.WorkItem });

    /// <summary>
    /// Unlinks the issue from the work item, when it is the one it is linked to.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportUnlinkedWorkItem @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySystem(context, ConversationEntryKind.UnlinkedFromWorkItem, (string)@event.WorkItemReference), issue =>
            issue.WorkItem.Reference == @event.WorkItemReference ? issue with { WorkItem = Unset.WorkItem } : issue);

    /// <summary>
    /// Records that the desk started work on the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportStartedWork @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySystem(context, ConversationEntryKind.WorkStarted), issue => IssueConversationTransitions.Move(issue, IssueState.InProgress));

    /// <summary>
    /// Records that the desk resolved the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportResolvedIssue @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySystem(context, ConversationEntryKind.Resolved, (string)@event.Resolution), issue => IssueConversationTransitions.Move(issue, IssueState.Resolved));

    /// <summary>
    /// Records the version that has the fix.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(ReleaseShippedFix @event, IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySystem(context, ConversationEntryKind.FixReleased, (string)@event.ReleaseVersion), issue => issue with { FixedInVersion = @event.ReleaseVersion });
}
