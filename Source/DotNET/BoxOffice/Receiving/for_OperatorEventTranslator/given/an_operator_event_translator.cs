// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Execution;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Receiving.for_OperatorEventTranslator.given;

/// <summary>
/// An <see cref="OperatorEventTranslator"/> receiving the decisions of the <c>Direct</c> operator in the default namespace's
/// inbox of the <c>Ledger</c> application, which has the tenant namespace <see cref="Tenants.Tenant"/>.
/// </summary>
public class an_operator_event_translator : Specification
{
    /// <summary>
    /// The application's event store.
    /// </summary>
    protected static readonly EventStoreName Application = "Ledger";

    /// <summary>
    /// The operator store the decisions come from.
    /// </summary>
    protected static readonly EventStoreName Operator = "Direct";

    /// <summary>
    /// The tenant's namespace.
    /// </summary>
    protected static readonly EventStoreNamespaceName Tenant = new(Tenants.Tenant.Value);

    /// <summary>
    /// When the operator decided.
    /// </summary>
    protected static readonly DateTimeOffset Occurred = new(2026, 10, 26, 14, 15, 0, TimeSpan.Zero);

    protected IEventStore _inbox;
    protected EventStoreForTesting _tenantStore;
    protected IChronicleClient _client;
    protected RecordingLogger<OperatorEventTranslator> _logger;
    protected OperatorEventTranslator _translator;
    protected CorrelationId _correlationId;
    protected IImmutableList<AppendedEvent> _recorded;

    /// <summary>
    /// Gets the namespaces the application has.
    /// </summary>
    protected virtual IEnumerable<EventStoreNamespaceName> Namespaces => [EventStoreNamespaceName.Default, Tenant];

    void Establish()
    {
        _inbox = CreateInbox();
        _tenantStore = new();
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Application, Arg.Is<EventStoreNamespaceName?>(_ => _ == EventStoreNamespaceName.Default)).Returns(_inbox);
        _client.GetEventStore(Application, Arg.Is<EventStoreNamespaceName?>(_ => _ == Tenant)).Returns(_tenantStore);
        _logger = new();
        _translator = new(_client, _logger);
        _correlationId = CorrelationId.New();
    }

    void Destroy()
    {
        (_inbox as IDisposable)?.Dispose();
        _tenantStore.Dispose();
    }

    /// <summary>
    /// Creates the event store of the namespace the operator's events arrive in.
    /// </summary>
    /// <returns>The event store.</returns>
    protected virtual IEventStore CreateInbox()
    {
        var inbox = Substitute.For<IEventStore>();
        inbox.GetNamespaces(Arg.Any<CancellationToken>()).Returns(_ => Namespaces);
        return inbox;
    }

    /// <summary>
    /// Creates the context of an operator event in the inbox.
    /// </summary>
    /// <param name="eventSource">The operator's event source: the organization or the offer.</param>
    /// <param name="sequenceNumber">The sequence number of the event in the inbox.</param>
    /// <returns>The context.</returns>
    protected EventContext InboxContext(EventSourceId eventSource, ulong sequenceNumber = 0) => EventContext.From(
        Application,
        EventStoreNamespaceName.Default,
        EventType.Unknown,
        EventSourceType.Default,
        eventSource,
        EventStreamType.All,
        EventStreamId.Default,
        sequenceNumber,
        _correlationId,
        Occurred);

    /// <summary>
    /// Creates the context of an operator event about the tenant's organization.
    /// </summary>
    /// <param name="sequenceNumber">The sequence number of the event in the inbox.</param>
    /// <returns>The context.</returns>
    protected EventContext OrganizationContext(ulong sequenceNumber = 0) => InboxContext(new(Tenants.Organization.Value.ToString("D")), sequenceNumber);

    /// <summary>
    /// Reads what is recorded in the tenant's stream in the tenant namespace.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadRecorded() =>
        _recorded = await _tenantStore.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, TenantStream.Id);
}
#endif
