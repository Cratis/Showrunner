// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Receiving;

/// <summary>
/// Records what the operator decided about audiences: turns a public event that arrived in the inbox into the matching
/// private fact and appends it to the stream of its level. A decision about a tenant or a user is recorded in the
/// namespace the tenant names; a decision about the system is recorded where the operator published it, the product's own
/// namespace.
/// </summary>
/// <remarks>
/// <para>
/// Chronicle's forwarder delivers an event at least once, so the same event can arrive twice, and the inbox can be handed
/// the same event again after a failure. The private fact is appended with an idempotency key derived from the event
/// itself - the operator store, the product, the event type, when it occurred, its correlation id and its content - so every
/// copy of the same event yields one fact. The fact keeps the time the operator decided, so the newest decision wins
/// whatever order the decisions arrive in.
/// </para>
/// <para>
/// A decision for another product is ignored. A decision that names no audience, or no tenant or user its level needs, is
/// not recorded. A decision about a tenant the product does not have is not recorded either: a decision never creates a
/// tenant, so the operator repeats it once the tenant exists.
/// </para>
/// <para>
/// Nothing here is read from the operator at request time. What is recorded stays what it was while the connection to the
/// operator is down, and the inbox catches up when it is back.
/// </para>
/// </remarks>
/// <param name="client">The Chronicle client, to reach the event log of the namespace a decision is recorded in.</param>
/// <param name="options">GuestList's options, which name the product.</param>
/// <param name="logger">The logger.</param>
public class GuestListInbox(IChronicleClient client, IOptions<GuestListOptions> options, ILogger<GuestListInbox> logger)
{
    /// <summary>
    /// Derives the idempotency key of an event from the operator store: the same for every copy of the event the forwarder
    /// delivers.
    /// </summary>
    /// <param name="operatorEvent">The public event the operator store sent.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="operatorStore">The operator store the event came from.</param>
    /// <returns>The key.</returns>
    public static IdempotencyKey KeyFor(object operatorEvent, EventContext context, EventStoreName operatorStore)
    {
        var identity = string.Join(
            '\n',
            operatorStore.Value,
            context.EventSourceId.Value,
            operatorEvent.GetType().FullName,
            context.Occurred.UtcTicks.ToString(CultureInfo.InvariantCulture),
            context.CorrelationId.Value.ToString("D"),
            JsonSerializer.Serialize(operatorEvent, operatorEvent.GetType()));
        return new($"guestlist-{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))}");
    }

    /// <summary>
    /// Receives an audience being granted.
    /// </summary>
    /// <param name="event">The public event.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="operatorStore">The operator store the event came from.</param>
    /// <returns>An awaitable task.</returns>
    public Task Receive(AudienceGranted @event, EventContext context, EventStoreName operatorStore) =>
        Record(@event, @event.Product, @event.Level, @event.TenantId, @event.UserId, @event.Audience, new AudienceGrantRecorded(@event.Level, @event.Audience), context, operatorStore);

    /// <summary>
    /// Receives an audience being withdrawn.
    /// </summary>
    /// <param name="event">The public event.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="operatorStore">The operator store the event came from.</param>
    /// <returns>An awaitable task.</returns>
    public Task Receive(AudienceWithdrawn @event, EventContext context, EventStoreName operatorStore) =>
        Record(@event, @event.Product, @event.Level, @event.TenantId, @event.UserId, @event.Audience, new AudienceWithdrawalRecorded(@event.Level, @event.Audience), context, operatorStore);

    static bool IsNamedFor(AudienceLevel level, TenantId tenant, UserId user) => level switch
    {
        AudienceLevel.Tenant => !string.IsNullOrWhiteSpace(tenant.Value),
        AudienceLevel.User => !string.IsNullOrWhiteSpace(tenant.Value) && !string.IsNullOrWhiteSpace(user.Value),
        AudienceLevel.System => true,
        _ => false
    };

    async Task Record(
        object operatorEvent,
        Product product,
        AudienceLevel level,
        TenantId tenant,
        UserId user,
        AudienceName audience,
        object fact,
        EventContext context,
        EventStoreName operatorStore)
    {
        var eventType = operatorEvent.GetType().Name;
        if (product != options.Value.Product)
        {
            logger.ForAnotherProduct(eventType, operatorStore, product.Value);
            return;
        }

        if (string.IsNullOrWhiteSpace(audience.Value) || !IsNamedFor(level, tenant, user))
        {
            logger.Malformed(eventType, operatorStore, level);
            return;
        }

        var inbox = await client.GetEventStore(context.EventStore, context.Namespace);
        var @namespace = context.Namespace;
        if (level != AudienceLevel.System)
        {
            var found = (await inbox.GetNamespaces()).Where(name => string.Equals(name.Value, tenant.Value, StringComparison.Ordinal)).Take(2).ToArray();
            if (found.Length != 1)
            {
                logger.UnknownTenant(eventType, operatorStore, tenant.Value);
                return;
            }

            @namespace = found[0];
        }

        var store = await client.GetEventStore(context.EventStore, @namespace);
        var eventSourceId = GuestListStreams.For(level, user);
        var result = await store.EventLog.AppendOnce(
            eventSourceId,
            fact,
            KeyFor(operatorEvent, context, operatorStore),
            correlationId: context.CorrelationId,
            occurred: context.Occurred);

        if (result.WasAlreadyAppended)
        {
            logger.AlreadyRecorded(fact.GetType().Name, eventSourceId, @namespace, operatorStore);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new AudienceNotRecorded(fact.GetType(), eventSourceId, @namespace, operatorStore, result);
        }
    }
}
