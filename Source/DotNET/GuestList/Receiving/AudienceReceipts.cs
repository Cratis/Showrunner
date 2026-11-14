// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Reactors;
using Cratis.Showrunner.GuestList.Contracts.Availability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReceivedDecision = Cratis.Showrunner.GuestList.Contracts.Availability.AudienceDecision;

namespace Cratis.Showrunner.GuestList.Receiving;

/// <summary>
/// Tells the operator that a decision reached the product: for every decision the product recorded, it publishes one
/// <see cref="AudienceDecisionReceived"/>. It follows what the product recorded and not what it was sent, so a decision
/// that is delivered again, or one the product ignored, such as one for a tenant it does not have, produces nothing.
/// It is not a heartbeat: nothing is published while nothing is decided.
/// </summary>
/// <param name="client">The <see cref="IChronicleClient"/>.</param>
/// <param name="options">The <see cref="GuestListOptions"/>.</param>
/// <param name="logger">The <see cref="ILogger{AudienceReceipts}"/>.</param>
public class AudienceReceipts(IChronicleClient client, IOptions<GuestListOptions> options, ILogger<AudienceReceipts> logger)
{
    const string UserStreamPrefix = "user:";

    readonly Lock _lock = new();
    bool _observed;

    /// <summary>
    /// Gets the id of the reactor that publishes the receipts.
    /// </summary>
    public static ReactorId Reactor { get; } = new("Cratis.Showrunner.GuestList.Receiving.AudienceReceipts");

    /// <summary>
    /// Gets the idempotency key of the receipt of a recorded decision: the same decision gets the same key, however
    /// often it is delivered to the reactor.
    /// </summary>
    /// <param name="context">The context of the recorded decision.</param>
    /// <returns>The key.</returns>
    public static IdempotencyKey KeyFor(EventContext context) =>
        new($"guestlist-received-{Uri.EscapeDataString(context.Namespace.Value)}-{Uri.EscapeDataString(context.EventSourceId.Value)}-{context.SequenceNumber.Value}");

    /// <summary>
    /// Starts publishing the receipts of what the product records in its event store, once.
    /// </summary>
    /// <param name="eventStore">The product's event store.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Observe(IEventStore eventStore)
    {
        lock (_lock)
        {
            if (_observed)
            {
                return;
            }
        }

        await eventStore.Reactors.Register(Reactor, reactor => reactor
            .NotReplayable()
            .On<AudienceGrantRecorded>((@event, context) => Receive(ReceivedDecision.Granted, @event.Level, @event.Audience, context))
            .On<AudienceWithdrawalRecorded>((@event, context) => Receive(ReceivedDecision.Withdrawn, @event.Level, @event.Audience, context)));

        lock (_lock)
        {
            _observed = true;
        }
    }

    /// <summary>
    /// Publishes the receipt of a decision the product recorded.
    /// </summary>
    /// <param name="decision">What was decided.</param>
    /// <param name="level">Whom it was decided for.</param>
    /// <param name="audience">The audience decided.</param>
    /// <param name="context">The context of the recorded decision: its occurred time is when the operator decided.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="AudienceReceiptNotPublished">The receipt could not be appended; the reactor delivers the decision again.</exception>
    public async Task Receive(ReceivedDecision decision, AudienceLevel level, AudienceName audience, EventContext context)
    {
        var tenant = level is AudienceLevel.System or AudienceLevel.NamedSystem ? TenantId.NotSet : new TenantId(context.Namespace.Value);
        var user = level == AudienceLevel.User && context.EventSourceId.Value.StartsWith(UserStreamPrefix, StringComparison.Ordinal)
            ? new UserId(context.EventSourceId.Value[UserStreamPrefix.Length..])
            : UserId.NotSet;

        var systemClient = level == AudienceLevel.NamedSystem && context.EventSourceId.Value.StartsWith(GuestListStreams.NamedSystemPrefix, StringComparison.Ordinal)
            ? new SystemClientId(context.EventSourceId.Value[GuestListStreams.NamedSystemPrefix.Length..])
            : SystemClientId.NotSet;

        var received = new AudienceDecisionReceived(options.Value.Product, options.Value.Environment, decision, level, tenant, user, audience, context.Occurred, systemClient);
        var store = await client.GetEventStore(context.EventStore, EventStoreNamespaceName.Default);
        using var routing = string.IsNullOrWhiteSpace(options.Value.OperatorAudience) ? null : EventTags.Audience(options.Value.OperatorAudience);
        var result = await store.GetEventSequence(EventSequenceId.Outbox).AppendOnce(options.Value.Product.Value, received, KeyFor(context), occurred: context.Occurred);

        if (result.WasAlreadyAppended || result.HasIdempotencyConflicts)
        {
            logger.AlreadyReceived(level, audience);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new AudienceReceiptNotPublished(level, audience, result);
        }
    }
}
