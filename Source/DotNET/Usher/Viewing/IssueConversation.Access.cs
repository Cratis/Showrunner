// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Viewing;

/// <summary>
/// The reducers of <see cref="IssueConversation"/> for access: withdrawing the issue, the grant, revocation and lapse of a grant, support's request and the sessions in which support signs in as the reporter.
/// </summary>
public partial record IssueConversation
{
    /// <summary>
    /// Withdraws the issue.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(ReporterWithdrewIssue @event, IssueConversation? current, EventContext context) =>
        IssueConversationAccess.Withdraw(current, context);

    /// <summary>
    /// Records the grant of access and answers support's request for it.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(ReporterGrantedAccess @event, IssueConversation? current, EventContext context) =>
        IssueConversationAccess.Grant(current, context, @event);

    /// <summary>
    /// Records that the reporter took access back.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(ReporterRevokedAccess @event, IssueConversation? current, EventContext context) =>
        IssueConversationAccess.Revoke(current, context);

    /// <summary>
    /// Records that the grant ended by its time passing, when the grant that lapsed is the current one.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(AccessGrantLapsed @event, IssueConversation? current, EventContext context) =>
        IssueConversationAccess.Lapse(current, context, @event);

    /// <summary>
    /// Records support's request for access.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportRequestedAccess @event, IssueConversation? current, EventContext context) =>
        IssueConversationAccess.Request(current, context, @event);

    /// <summary>
    /// Starts a session in which support signed in as the reporter.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportSignedInAsReporter @event, IssueConversation? current, EventContext context) =>
        IssueConversationAccess.Start(current, context, @event);

    /// <summary>
    /// Ends a session in which support signed in as the reporter.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static IssueConversation? Reduce(SupportSessionEnded @event, IssueConversation? current, EventContext context) =>
        IssueConversationAccess.End(current, context, @event);
}
