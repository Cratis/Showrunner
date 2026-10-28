// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.KnownIssues;
using Cratis.Showrunner.Usher.Contracts.Lifecycle;
using Cratis.Showrunner.Usher.Contracts.WorkItems;

namespace Cratis.Showrunner.Usher.Receiving;

/// <summary>
/// Support triaged the issue: the desk looked at it and will decide whether to accept it. Translated from
/// <c>IssueTriaged</c>.
/// </summary>
[EventType]
public record SupportTriagedIssue;

/// <summary>
/// Support accepted the issue as something to fix. Translated from <c>IssueAccepted</c>.
/// </summary>
[EventType]
public record SupportAcceptedIssue;

/// <summary>
/// Support decided not to fix the issue. Translated from <c>IssueClosedAsWontFix</c>.
/// </summary>
/// <param name="Reason">Why support will not fix it.</param>
[EventType]
public record SupportDeclinedToFix(WontFixReason Reason);

/// <summary>
/// Support asked the reporter a question. The question stays open until the reporter answers it. Translated from
/// <c>QuestionAsked</c>.
/// </summary>
/// <param name="QuestionId">The question, as the desk minted it.</param>
/// <param name="Question">What support asked.</param>
/// <param name="Author">Who asked, as the desk lets the reporter see them.</param>
[EventType]
public record SupportAskedQuestion(QuestionId QuestionId, QuestionText Question, SupportAuthor Author);

/// <summary>
/// Support replied to the reporter. Translated from <c>ReplySent</c>.
/// </summary>
/// <param name="Reply">What support wrote.</param>
/// <param name="Author">Who wrote, as the desk lets the reporter see them.</param>
[EventType]
public record SupportReplied(ReplyText Reply, SupportAuthor Author);

/// <summary>
/// Support linked the issue to a known issue, the desk's group of duplicate reports. Translated from
/// <c>IssueLinkedToKnownIssue</c>.
/// </summary>
/// <param name="KnownIssueId">The known issue.</param>
/// <param name="Title">The title of the known issue.</param>
[EventType]
public record SupportLinkedIssueToKnownIssue(KnownIssueId KnownIssueId, KnownIssueTitle Title);

/// <summary>
/// Support unlinked the issue from a known issue. Translated from <c>IssueUnlinkedFromKnownIssue</c>.
/// </summary>
/// <param name="KnownIssueId">The known issue it was unlinked from.</param>
[EventType]
public record SupportUnlinkedIssue(KnownIssueId KnownIssueId);

/// <summary>
/// Support linked the issue to a work item in the application's issue tracker. Translated from
/// <c>IssueLinkedToWorkItem</c>.
/// </summary>
/// <param name="WorkItem">The work item.</param>
[EventType]
public record SupportLinkedIssueToWorkItem(WorkItem WorkItem);

/// <summary>
/// Support unlinked the issue from a work item. Translated from <c>IssueUnlinkedFromWorkItem</c>.
/// </summary>
/// <param name="WorkItemReference">The work item it was unlinked from.</param>
[EventType]
public record SupportUnlinkedWorkItem(WorkItemReference WorkItemReference);

/// <summary>
/// Support started working on the issue. Translated from <c>WorkStarted</c>.
/// </summary>
[EventType]
public record SupportStartedWork;

/// <summary>
/// Support resolved the issue. The release that ships the fix follows as its own fact,
/// <see cref="ReleaseShippedFix"/>, often later. Translated from <c>IssueResolved</c>.
/// </summary>
/// <param name="Resolution">How support resolved it.</param>
[EventType]
public record SupportResolvedIssue(Resolution Resolution);

/// <summary>
/// Support asked the reporter for access to sign in as them. Translated from <c>AccessRequested</c>.
/// </summary>
/// <param name="Reason">Why support needs access.</param>
[EventType]
public record SupportRequestedAccess(AccessRequestReason Reason);

/// <summary>
/// A released version of the application closed the work item the issue is linked to, so the reporter knows which
/// version has the fix. It is separate from <see cref="SupportResolvedIssue"/> because a fix is often resolved when
/// merged and released later. Translated from <c>IssueFixedInRelease</c>.
/// </summary>
/// <param name="ReleaseVersion">The version that has the fix.</param>
/// <param name="WorkItemReference">The work item the release closed.</param>
[EventType]
public record ReleaseShippedFix(ReleaseVersion ReleaseVersion, WorkItemReference WorkItemReference);
