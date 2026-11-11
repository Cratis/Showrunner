// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.BoxOffice.Receiving;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning;

/// <summary>
/// Handles what the operator tells the product about a tenant's life: that it is registered, so the product creates it
/// and reports it ready, and that it is closing.
/// </summary>
/// <param name="client">The <see cref="IChronicleClient"/> to record and report with.</param>
/// <param name="provisioner">The product's <see cref="IProvisionTenants"/>.</param>
/// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
/// <param name="closing">The product's <see cref="IClosingTenants"/>, when it takes part in tearing tenants down.</param>
public class TenantRegistrations(
    IChronicleClient client,
    IProvisionTenants provisioner,
    ILogger<TenantRegistrations> logger,
    IClosingTenants? closing = null)
{
    /// <summary>
    /// Gets the key a tenant's ready report is recorded with, so it is reported once however often the registration is delivered.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <returns>The <see cref="IdempotencyKey"/>.</returns>
    public static IdempotencyKey ProvisionedKeyFor(TenantId tenant) => new($"provisioned-{tenant.Value}");

    /// <summary>
    /// Gets the key a tenant's refusal is recorded with.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <returns>The <see cref="IdempotencyKey"/>.</returns>
    public static IdempotencyKey NotProvisionedKeyFor(TenantId tenant) => new($"not-provisioned-{tenant.Value}");

    /// <summary>
    /// The operator registered a tenant: create it, then report the answer to the operator under the organization.
    /// </summary>
    /// <param name="registered">The <see cref="TenantRegistered"/> event.</param>
    /// <param name="context">The <see cref="EventContext"/> of the event, whose event source is the operator's organization.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the work.</param>
    /// <returns>A <see cref="Task"/> for continuation.</returns>
    public async Task Registered(TenantRegistered registered, EventContext context, CancellationToken cancellationToken = default)
    {
        var answer = await provisioner.Provision(
            new(registered.TenantId, registered.Name, registered.Creator, registered.CreatorRole),
            cancellationToken);

        var arrival = new Arrival(registered.TenantId, context);
        var product = await client.GetEventStore(context.EventStore, context.Namespace);
        var outbox = product.GetEventSequence(EventSequenceId.Outbox);

        if (!answer.IsProvisioned)
        {
            logger.Refused(registered.TenantId, answer.Reason);
            var refused = new TenantNotProvisioned(registered.TenantId, answer.Reason);
            await Report(outbox, arrival, refused, NotProvisionedKeyFor(registered.TenantId));
            return;
        }

        await Remember(registered, arrival);
        await Report(outbox, arrival, new TenantProvisioned(registered.TenantId), ProvisionedKeyFor(registered.TenantId));
        logger.Provisioned(registered.TenantId);
    }

    /// <summary>
    /// The operator is closing a tenant: tell the product, when it takes part.
    /// </summary>
    /// <param name="closingTenant">The <see cref="TenantClosing"/> event.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the work.</param>
    /// <returns>A <see cref="Task"/> for continuation.</returns>
    public async Task Closing(TenantClosing closingTenant, CancellationToken cancellationToken = default)
    {
        if (closing is null)
        {
            logger.ClosingIgnored(closingTenant.TenantId);
            return;
        }

        await closing.Closing(closingTenant.TenantId, cancellationToken);
        logger.Closing(closingTenant.TenantId);
    }

    static bool IsRecorded(AppendResult result) => result.IsSuccess || result.WasAlreadyAppended;

    /// <summary>
    /// The tenant's own namespace learns which organization it is and who created it, so the product can report seats and
    /// activity under the organization and tell a person who to ask for a seat. Both are keyed, so a repeat records nothing.
    /// </summary>
    /// <param name="registered">The <see cref="TenantRegistered"/> event.</param>
    /// <param name="arrival">The tenant and the <see cref="EventContext"/> of the event.</param>
    /// <returns>A <see cref="Task"/> for continuation.</returns>
    async Task Remember(TenantRegistered registered, Arrival arrival)
    {
        var context = arrival.Context;
        var tenant = await client.GetEventStore(context.EventStore, new EventStoreNamespaceName(registered.TenantId.Value));

        if (Guid.TryParse(context.EventSourceId.Value, out var organization))
        {
            await Record(
                tenant,
                arrival,
                new TenantOrganizationIdentified(new OrganizationId(organization)),
                OperatorEventTranslator.OrganizationKeyFor(context.EventSourceId));
        }

        await Record(
            tenant,
            arrival,
            new TenantCreatorIdentified(registered.Creator, registered.CreatorRole),
            new($"creator-{registered.TenantId.Value}"));
    }

    async Task Record(IEventStore tenant, Arrival arrival, object fact, IdempotencyKey key)
    {
        var context = arrival.Context;
        var result = await tenant.EventLog.AppendOnce(TenantStream.Id, fact, key, correlationId: context.CorrelationId, occurred: context.Occurred);
        if (!IsRecorded(result) && !result.HasIdempotencyConflicts)
        {
            throw new TenantNotRecorded(fact.GetType(), arrival.Tenant, result);
        }
    }

    async Task Report(IEventSequence outbox, Arrival arrival, object fact, IdempotencyKey key)
    {
        var context = arrival.Context;
        var result = await outbox.AppendOnce(context.EventSourceId, fact, key, correlationId: context.CorrelationId);
        if (result.HasIdempotencyConflicts)
        {
            logger.AnsweredDifferently(arrival.Tenant);
            return;
        }

        if (!IsRecorded(result))
        {
            throw new TenantNotRecorded(fact.GetType(), arrival.Tenant, result);
        }
    }

    readonly record struct Arrival(TenantId Tenant, EventContext Context);
}
