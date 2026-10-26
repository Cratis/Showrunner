// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.KnownIssues;
using Cratis.Showrunner.Usher.Contracts.Lifecycle;
using Cratis.Showrunner.Usher.Contracts.WorkItems;

namespace Cratis.Showrunner.Usher.Receiving;

/// <summary>
/// Turns the desk's public events into the application's private <c>Support…</c> facts. Each arm is one row of the
/// mapping table in the contracts documentation.
/// </summary>
public static class PrivateEvents
{
    /// <summary>
    /// Gets every public event the desk sends, which is what Usher subscribes to and translates.
    /// </summary>
    public static readonly Type[] FromDesk =
    [
        typeof(IssueTriaged),
        typeof(IssueAccepted),
        typeof(IssueClosedAsWontFix),
        typeof(QuestionAsked),
        typeof(ReplySent),
        typeof(IssueLinkedToKnownIssue),
        typeof(IssueUnlinkedFromKnownIssue),
        typeof(IssueLinkedToWorkItem),
        typeof(IssueUnlinkedFromWorkItem),
        typeof(WorkStarted),
        typeof(IssueResolved),
        typeof(IssueFixedInRelease),
        typeof(AccessRequested),
    ];

    /// <summary>
    /// Translates a public event from the desk.
    /// </summary>
    /// <param name="deskEvent">The public event.</param>
    /// <returns>The tenant it is for and the private event; <see langword="null"/> when it is not an event from the desk.</returns>
    public static (TenantId TenantId, object Event)? For(object deskEvent) => deskEvent switch
    {
        IssueTriaged e => (e.TenantId, new SupportTriagedIssue()),
        IssueAccepted e => (e.TenantId, new SupportAcceptedIssue()),
        IssueClosedAsWontFix e => (e.TenantId, new SupportDeclinedToFix(e.Reason)),
        QuestionAsked e => (e.TenantId, new SupportAskedQuestion(e.QuestionId, e.Question, e.Author)),
        ReplySent e => (e.TenantId, new SupportReplied(e.Reply, e.Author)),
        IssueLinkedToKnownIssue e => (e.TenantId, new SupportLinkedIssueToKnownIssue(e.KnownIssueId, e.Title)),
        IssueUnlinkedFromKnownIssue e => (e.TenantId, new SupportUnlinkedIssue(e.KnownIssueId)),
        IssueLinkedToWorkItem e => (e.TenantId, new SupportLinkedIssueToWorkItem(e.WorkItem)),
        IssueUnlinkedFromWorkItem e => (e.TenantId, new SupportUnlinkedWorkItem(e.WorkItemReference)),
        WorkStarted e => (e.TenantId, new SupportStartedWork()),
        IssueResolved e => (e.TenantId, new SupportResolvedIssue(e.Resolution)),
        IssueFixedInRelease e => (e.TenantId, new ReleaseShippedFix(e.ReleaseVersion, e.WorkItemReference)),
        AccessRequested e => (e.TenantId, new SupportRequestedAccess(e.Reason)),
        _ => null
    };
}
