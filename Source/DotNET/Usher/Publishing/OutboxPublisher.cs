// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Reactors;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Conversation.AddDetails;
using Cratis.Showrunner.Usher.Conversation.AnswerQuestion;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Lifecycle.ReopenIssue;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;
using Cratis.Showrunner.Usher.Reporting.ReportError;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Publishing;

/// <summary>
/// Publishes what the reporter did, and the lapse of a grant, to the support desk: each private fact becomes its public
/// event in the outbox of the namespace it happened in, under the same issue id. The desk reads the outbox through its
/// own subscription.
/// </summary>
/// <remarks>
/// <para>
/// The <c>TenantId</c> of every public event is the namespace the private fact was recorded in. The public event keeps
/// the private fact's correlation id, compliance subject and the time it happened.
/// </para>
/// <para>
/// Chronicle delivers an event at least once: a failed partition is recovered by delivering it again. Each publication
/// is appended with an idempotency key derived from the delivery, so a delivery that is repeated - recovered, retried
/// by another instance, or replayed - finds its publication already in the outbox and appends nothing. The reactor is
/// also <see cref="OnceOnlyAttribute">not replayable</see>, so a replay does not even try. A refused append throws, so
/// the partition fails and the publication is retried rather than lost.
/// </para>
/// </remarks>
/// <param name="client">The Chronicle client, to reach the outbox of the namespace the fact was recorded in.</param>
/// <param name="logger">The logger.</param>
[Reactor(Id)]
[OnceOnly]
public class OutboxPublisher(IChronicleClient client, ILogger<OutboxPublisher> logger) : IReactor
{
    /// <summary>
    /// The stable identity of the reactor; its position in the event log and its idempotency keys hang on it.
    /// </summary>
    public const string Id = "Cratis.Showrunner.Usher.Publishing.OutboxPublisher";

    /// <summary>
    /// Publishes a reported error as <c>ErrorReported</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task ErrorFlagged(ReporterFlaggedError @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes a problem described by hand as <c>IssueReported</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task IssueDescribed(ReporterDescribedIssue @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes the reporter sharing who they are as <c>IssueAttributedToReporter</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task IdentityShared(ReporterSharedIdentity @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes details the reporter added as <c>DetailsAdded</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task AddedDetails(ReporterAddedDetails @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes an answer as <c>QuestionAnswered</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task AnsweredQuestion(ReporterAnsweredQuestion @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes a reopened issue as <c>IssueReopened</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task ReopenedIssue(ReporterReopenedIssue @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes a withdrawn issue as <c>IssueWithdrawn</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task WithdrewIssue(ReporterWithdrewIssue @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes a grant of access as <c>AccessGranted</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task GrantedAccess(ReporterGrantedAccess @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes revoked access as <c>AccessRevoked</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task RevokedAccess(ReporterRevokedAccess @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes a lapsed grant as <c>AccessExpired</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task GrantLapsed(AccessGrantLapsed @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    static TenantId TenantOf(EventContext context) => new(context.Namespace.Value);

    async Task Publish(object publicEvent, EventContext context, ReactorDelivery delivery)
    {
        var eventStore = await client.GetEventStore(context.EventStore, context.Namespace);
        var result = await eventStore.GetEventSequence(EventSequenceId.Outbox).AppendOnce(
            context.EventSourceId,
            publicEvent,
            delivery.IdempotencyKeyFor(nameof(Publish)),
            correlationId: context.CorrelationId,
            occurred: context.Occurred,
            subject: context.Subject);

        if (result.HasIdempotencyConflicts)
        {
            // The delivery already published a different public event - an earlier version of Usher mapped the fact
            // differently. The fact is published; publishing it again would tell the desk twice.
            logger.AlreadyPublishedDifferently(publicEvent.GetType().Name, context.EventSourceId, context.Namespace);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new PublicationRefused(publicEvent.GetType(), context.EventSourceId, context.Namespace, result);
        }
    }
}
