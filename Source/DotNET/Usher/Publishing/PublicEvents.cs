// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Lifecycle.ReopenIssue;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;
using Cratis.Showrunner.Usher.Reporting.ReportError;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;

namespace Cratis.Showrunner.Usher.Publishing;

/// <summary>
/// Turns the reporter's private facts, and the lapse of a grant, into the public events the desk reads. Each method is
/// one row of the mapping table in the contracts documentation.
/// </summary>
/// <remarks>
/// The public events carry what the desk needs and nothing else. <c>ReporterSubject</c> on the events that open an
/// issue is never published: the desk learns who reported only from <see cref="IssueAttributedToReporter"/>.
/// </remarks>
public static class PublicEvents
{
    /// <summary>
    /// Maps a reported error.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the issue belongs to.</param>
    /// <returns>The public event.</returns>
    public static ErrorReported For(ReporterFlaggedError @event, TenantId tenantId) => new(
        tenantId,
        @event.Location,
        @event.CorrelationIds,
        @event.StackFrames,
        @event.ErrorMessage,
        @event.ErrorType,
        @event.Fingerprint,
        @event.ApplicationName,
        @event.ApplicationVersion);

    /// <summary>
    /// Maps a problem described by hand.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the issue belongs to.</param>
    /// <returns>The public event.</returns>
    public static IssueReported For(ReporterDescribedIssue @event, TenantId tenantId) => new(
        tenantId,
        @event.Location,
        @event.CorrelationIds,
        @event.Summary,
        @event.Description,
        @event.ApplicationName,
        @event.ApplicationVersion);

    /// <summary>
    /// Maps the reporter sharing who they are.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the issue belongs to.</param>
    /// <returns>The public event.</returns>
    public static IssueAttributedToReporter For(ReporterSharedIdentity @event, TenantId tenantId) => new(tenantId, @event.Reporter);

    /// <summary>
    /// Maps details the reporter added.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the issue belongs to.</param>
    /// <returns>The public event.</returns>
    public static DetailsAdded For(ReporterAddedDetails @event, TenantId tenantId) => new(tenantId, @event.Details);

    /// <summary>
    /// Maps an answer to a question support asked.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the issue belongs to.</param>
    /// <returns>The public event.</returns>
    public static QuestionAnswered For(ReporterAnsweredQuestion @event, TenantId tenantId) => new(tenantId, @event.QuestionId, @event.Answer);

    /// <summary>
    /// Maps the reporter reopening a resolved issue.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the issue belongs to.</param>
    /// <returns>The public event.</returns>
    public static IssueReopened For(ReporterReopenedIssue @event, TenantId tenantId) => new(tenantId, @event.Reason);

    /// <summary>
    /// Maps the reporter withdrawing the issue.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the issue belongs to.</param>
    /// <returns>The public event.</returns>
    public static IssueWithdrawn For(ReporterWithdrewIssue @event, TenantId tenantId) => new(tenantId);

    /// <summary>
    /// Maps a grant of access.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the issue belongs to.</param>
    /// <returns>The public event.</returns>
    public static AccessGranted For(ReporterGrantedAccess @event, TenantId tenantId) => new(tenantId, @event.ValidUntil);

    /// <summary>
    /// Maps the reporter revoking access.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the issue belongs to.</param>
    /// <returns>The public event.</returns>
    public static AccessRevoked For(ReporterRevokedAccess @event, TenantId tenantId) => new(tenantId);

    /// <summary>
    /// Maps a grant that lapsed when its time passed.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the issue belongs to.</param>
    /// <returns>The public event.</returns>
    public static AccessExpired For(AccessGrantLapsed @event, TenantId tenantId) => new(tenantId);
}
