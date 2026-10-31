// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.BoxOffice.Tenancy;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Receiving;

/// <summary>
/// Records the operator's decisions as private facts. A decision about a tenant is recorded in the namespace the tenant
/// names, in the tenant's stream; a decision about the product is recorded where the operator published it.
/// </summary>
/// <param name="client">The <see cref="IChronicleClient"/>.</param>
/// <param name="logger">The <see cref="ILogger{T}"/>.</param>
public class OperatorEventTranslator(IChronicleClient client, ILogger<OperatorEventTranslator> logger)
{
    /// <summary>
    /// Derives the idempotency key of a decision: the same for every copy of it the forwarder delivers.
    /// </summary>
    /// <param name="decision">The public event the operator sent.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="operatorStore">The operator store the event came from.</param>
    /// <returns>The key.</returns>
    public static IdempotencyKey KeyFor(object decision, EventContext context, EventStoreName operatorStore)
    {
        var identity = string.Join(
            '\n',
            operatorStore.Value,
            context.EventSourceId.Value,
            decision.GetType().FullName,
            context.Occurred.UtcTicks.ToString(CultureInfo.InvariantCulture),
            context.CorrelationId.Value.ToString("D"),
            JsonSerializer.Serialize(decision, decision.GetType()));
        return new($"boxoffice-operator-{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))}");
    }

    /// <summary>
    /// Derives the idempotency key of the fact that says which organization a tenant is.
    /// </summary>
    /// <param name="organization">The operator's identity for the organization.</param>
    /// <returns>The key.</returns>
    public static IdempotencyKey OrganizationKeyFor(EventSourceId organization) => new($"boxoffice-organization-{organization.Value}");

    /// <summary>
    /// Records one decision.
    /// </summary>
    /// <param name="decision">The public event the operator sent.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="operatorStore">The operator store the event came from.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="OperatorFactNotRecorded">The event log refused the private fact.</exception>
    public async Task Translate(object decision, EventContext context, EventStoreName operatorStore)
    {
        var eventType = decision.GetType().Name;
        if (OperatorDecisions.ForProduct(decision) is { } productFact)
        {
            var product = await client.GetEventStore(context.EventStore, context.Namespace);
            await Record(product, context.EventSourceId, productFact, KeyFor(decision, context, operatorStore), context, operatorStore);
            return;
        }

        if (OperatorDecisions.ForTenant(decision) is not { } translated)
        {
            logger.NotADecision(eventType, operatorStore);
            return;
        }

        var (tenantId, fact) = translated;
        if (string.IsNullOrWhiteSpace(tenantId.Value))
        {
            logger.NoTenant(eventType, context.EventSourceId, operatorStore);
            return;
        }

        var inbox = await client.GetEventStore(context.EventStore, context.Namespace);
        var found = TenantNamespaces.Find(await inbox.GetNamespaces(), tenantId);
        if (found.Match == TenantNamespaceMatch.Ambiguous)
        {
            logger.AmbiguousTenant(eventType, context.EventSourceId, tenantId, operatorStore);
            return;
        }

        if (found.Namespace is not { } @namespace)
        {
            logger.UnknownTenant(eventType, context.EventSourceId, tenantId, operatorStore);
            return;
        }

        var tenant = await client.GetEventStore(context.EventStore, @namespace);
        if (Guid.TryParse(context.EventSourceId.Value, out var organization))
        {
            await Record(
                tenant,
                TenantStream.Id,
                new TenantOrganizationIdentified(new OrganizationId(organization)),
                OrganizationKeyFor(context.EventSourceId),
                context,
                operatorStore);
        }

        await Record(tenant, TenantStream.Id, fact, KeyFor(decision, context, operatorStore), context, operatorStore);
    }

    async Task Record(IEventStore store, EventSourceId eventSourceId, object fact, IdempotencyKey key, EventContext context, EventStoreName operatorStore)
    {
        var result = await store.EventLog.AppendOnce(
            eventSourceId,
            fact,
            key,
            correlationId: context.CorrelationId,
            occurred: context.Occurred);

        if (result.WasAlreadyAppended)
        {
            logger.AlreadyRecorded(fact.GetType().Name, eventSourceId, store.Namespace, operatorStore);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new OperatorFactNotRecorded(fact.GetType(), eventSourceId, store.Namespace, result);
        }
    }
}
