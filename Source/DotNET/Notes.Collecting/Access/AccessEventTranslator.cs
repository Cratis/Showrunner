// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Collecting.Tenancy;
using Cratis.Showrunner.Notes.Contracts.Access;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Collecting.Access;

/// <summary>
/// Records what a desk decided about who may give feedback: turns a public event it sent into the matching private
/// <c>FeedbackAccess…Received</c> fact and appends it to the event log of the tenant namespace that the event's
/// <c>TenantId</c> names, under the user's <see cref="FeedbackGiverId"/> and with the user as the compliance subject. The
/// collector serves the users these facts leave granted to the applications that run their own kernel.
/// </summary>
/// <remarks>
/// <para>
/// The desk is trusted to name the tenant - it is one of the configured access stores - and to decide: a grant for a user
/// who has no notes is the normal case, so unlike an answer to a note nothing has to exist first. An event for a namespace the
/// collecting store does not have is logged and skipped: it changes nothing and failing on it would stop the desk's later events
/// behind it.
/// </para>
/// <para>
/// The private fact keeps the time the desk recorded it as its <c>Occurred</c>, which is how the latest decision wins even
/// when a grant and a revocation of one user are delivered out of order, and its correlation id.
/// </para>
/// <para>
/// Chronicle's forwarder delivers an event at least once. The private fact is appended with an idempotency key derived
/// from the event itself - the access store, the event type, the user, when it occurred, its correlation id and its content -
/// so every copy of the same event yields one private fact.
/// </para>
/// </remarks>
/// <param name="client">The Chronicle client, to reach the event log of the tenant namespace.</param>
/// <param name="logger">The logger.</param>
public class AccessEventTranslator(IChronicleClient client, ILogger<AccessEventTranslator> logger)
{
    /// <summary>
    /// Derives the idempotency key of an event from a desk: the same for every copy of the event the forwarder delivers.
    /// </summary>
    /// <param name="deskEvent">The public event the desk sent.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="accessStore">The access store the event came from.</param>
    /// <returns>The key.</returns>
    public static IdempotencyKey KeyFor(object deskEvent, EventContext context, EventStoreName accessStore)
    {
        var identity = string.Join(
            '\n',
            accessStore.Value,
            context.EventSourceId.Value,
            deskEvent.GetType().FullName,
            context.Occurred.UtcTicks.ToString(CultureInfo.InvariantCulture),
            context.CorrelationId.Value.ToString("D"),
            JsonSerializer.Serialize(deskEvent, deskEvent.GetType()));
        return new($"notes-collecting-access-{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))}");
    }

    /// <summary>
    /// Translates and records one event from a desk.
    /// </summary>
    /// <param name="deskEvent">The public event the desk sent.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="accessStore">The access store the event came from.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="AccessFactNotRecorded">The event log refused the private fact.</exception>
    public async Task Translate(object deskEvent, EventContext context, EventStoreName accessStore)
    {
        if (AccessPrivateEvents.For(deskEvent) is not { } translated)
        {
            logger.NotAnAccessEvent(deskEvent.GetType().Name, accessStore);
            return;
        }

        var (tenantId, giver, privateEvent) = translated;

        var eventType = deskEvent.GetType().Name;
        if (string.IsNullOrWhiteSpace(tenantId.Value))
        {
            logger.NoTenant(eventType, accessStore);
            return;
        }

        if (string.IsNullOrWhiteSpace(giver.Subject.Value))
        {
            logger.NoUser(eventType, accessStore);
            return;
        }

        var inbox = await client.GetEventStore(context.EventStore, context.Namespace);
        var namespaces = await inbox.GetNamespaces();
        var found = TenantNamespaces.Find(namespaces, tenantId);
        if (found.Match == TenantNamespaceMatch.Ambiguous)
        {
            logger.AmbiguousTenant(eventType, tenantId, accessStore);
            return;
        }

        if (found.Namespace is not { } @namespace)
        {
            logger.UnknownTenant(eventType, tenantId, accessStore);
            return;
        }

        var tenant = await client.GetEventStore(context.EventStore, @namespace);
        var giverId = (EventSourceId)FeedbackGiverId.For(giver);
        var result = await tenant.EventLog.AppendOnce(
            giverId,
            privateEvent,
            KeyFor(deskEvent, context, accessStore),
            correlationId: context.CorrelationId,
            occurred: context.Occurred,
            subject: FeedbackGiverId.SubjectOf(giver));

        if (result.WasAlreadyAppended)
        {
            logger.AlreadyRecorded(eventType, giverId, tenantId, accessStore);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new AccessFactNotRecorded(privateEvent.GetType(), giverId, @namespace, result);
        }
    }
}
