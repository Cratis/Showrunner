// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Issues;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Viewing;

/// <summary>
/// Records access and session changes in the reporter's conversation.
/// </summary>
internal static class IssueConversationAccess
{
    /// <summary>
    /// Withdraws the issue and ends an active grant.
    /// </summary>
    /// <param name="current">The current conversation.</param>
    /// <param name="context">The withdrawal's context.</param>
    /// <returns>The next conversation.</returns>
    internal static IssueConversation? Withdraw(IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.ByReporter(context, ConversationEntryKind.Withdrawn), issue => IssueConversationTransitions.Move(issue, IssueState.Withdrawn) with
        {
            Access = issue.Access.State == AccessState.Granted ? issue.Access with { State = AccessState.EndedByWithdrawal } : issue.Access
        });

    /// <summary>
    /// Records a grant and answers the pending access request.
    /// </summary>
    /// <param name="current">The current conversation.</param>
    /// <param name="context">The grant's context.</param>
    /// <param name="event">The grant.</param>
    /// <returns>The next conversation.</returns>
    internal static IssueConversation? Grant(IssueConversation? current, EventContext context, ReporterGrantedAccess @event) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.ByReporter(context, ConversationEntryKind.AccessGranted), issue => issue with
        {
            Access = issue.Access with { State = AccessState.Granted, ValidUntil = @event.ValidUntil, Request = AccessRequest.None }
        });

    /// <summary>
    /// Records the reporter's revocation.
    /// </summary>
    /// <param name="current">The current conversation.</param>
    /// <param name="context">The revocation's context.</param>
    /// <returns>The next conversation.</returns>
    internal static IssueConversation? Revoke(IssueConversation? current, EventContext context) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.ByReporter(context, ConversationEntryKind.AccessRevoked), issue => issue with
        {
            Access = issue.Access with { State = AccessState.Revoked }
        });

    /// <summary>
    /// Records a lapse only for the current active grant.
    /// </summary>
    /// <param name="current">The current conversation.</param>
    /// <param name="context">The lapse's context.</param>
    /// <param name="event">The grant that lapsed.</param>
    /// <returns>The next conversation.</returns>
    internal static IssueConversation? Lapse(IssueConversation? current, EventContext context, AccessGrantLapsed @event) =>
        current is null || current.Access.State != AccessState.Granted || current.Access.ValidUntil != @event.ValidUntil
            ? current
            : IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySystem(context, ConversationEntryKind.AccessLapsed), issue => issue with
            {
                Access = issue.Access with { State = AccessState.Lapsed }
            });

    /// <summary>
    /// Records support's pending access request.
    /// </summary>
    /// <param name="current">The current conversation.</param>
    /// <param name="context">The request's context.</param>
    /// <param name="event">The access request.</param>
    /// <returns>The next conversation.</returns>
    internal static IssueConversation? Request(IssueConversation? current, EventContext context, SupportRequestedAccess @event) =>
        IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySystem(context, ConversationEntryKind.AccessRequested, (string)@event.Reason), issue => issue with
        {
            Access = issue.Access with { Request = new(true, @event.Reason) }
        });

    /// <summary>
    /// Records the start of a session once.
    /// </summary>
    /// <param name="current">The current conversation.</param>
    /// <param name="context">The start's context.</param>
    /// <param name="event">The session that started.</param>
    /// <returns>The next conversation.</returns>
    internal static IssueConversation? Start(IssueConversation? current, EventContext context, SupportSignedInAsReporter @event) =>
        current?.Access.Sessions.Any(session => session.SessionId == @event.SessionId) != false
            ? current
            : IssueConversationTransitions.Add(current, context, IssueConversationTransitions.BySupport(context, ConversationEntryKind.SupportSignedIn, ConversationEntryText.NotSet, @event.Author) with { Session = @event.SessionId }, issue => issue with
            {
                Access = issue.Access with
                {
                    Sessions = [.. issue.Access.Sessions, new SupportSession(@event.SessionId, @event.Author, context.Occurred, false, Unset.Time, SupportActorKey.For(@event.Actor.Subject))]
                }
            });

    /// <summary>
    /// Ends a known open session once, keeping its original author.
    /// </summary>
    /// <param name="current">The current conversation.</param>
    /// <param name="context">The end's context.</param>
    /// <param name="event">The session that ended.</param>
    /// <returns>The next conversation.</returns>
    internal static IssueConversation? End(IssueConversation? current, EventContext context, SupportSessionEnded @event)
    {
        var session = current?.Access.Sessions.FirstOrDefault(session => session.SessionId == @event.SessionId);
        if (session is not { Ended: false })
        {
            return current;
        }

        var entry = IssueConversationTransitions.BySupport(context, ConversationEntryKind.SupportSessionEnded, ConversationEntryText.NotSet, session.Author) with { Session = @event.SessionId };
        return IssueConversationTransitions.Add(current, context, entry, issue => issue with
        {
            Access = issue.Access with
            {
                Sessions = [.. issue.Access.Sessions.Select(_ => _.SessionId == @event.SessionId ? _ with { Ended = true, EndedAt = context.Occurred } : _)]
            }
        });
    }
}
