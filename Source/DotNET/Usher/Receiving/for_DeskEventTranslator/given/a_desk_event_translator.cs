// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Execution;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.given;

/// <summary>
/// A <see cref="DeskEventTranslator"/> receiving events from the <c>Direct</c> desk in the default namespace's inbox
/// of the <c>Ledger</c> application, which has the tenant namespace <see cref="Tenant"/>.
/// </summary>
public class a_desk_event_translator : Specification
{
    /// <summary>
    /// The application's event store.
    /// </summary>
    protected static readonly EventStoreName Application = "Ledger";

    /// <summary>
    /// The desk store the events come from.
    /// </summary>
    protected static readonly EventStoreName Desk = "Direct";

    /// <summary>
    /// The tenant namespace the issues are in.
    /// </summary>
    protected static readonly EventStoreNamespaceName Tenant = "fjord-logistics";

    /// <summary>
    /// When the desk recorded its events.
    /// </summary>
    protected static readonly DateTimeOffset Occurred = new(2026, 10, 26, 14, 15, 0, TimeSpan.Zero);

    protected IEventStore _inbox;
    protected EventStoreForTesting _tenantStore;
    protected IChronicleClient _client;
    protected RecordingLogger<DeskEventTranslator> _logger;
    protected DeskEventTranslator _translator;
    protected IssueId _issueId;
    protected CorrelationId _correlationId;
    protected IImmutableList<AppendedEvent> _recorded;

    /// <summary>
    /// Gets the namespaces the application has.
    /// </summary>
    protected virtual IEnumerable<EventStoreNamespaceName> Namespaces => [EventStoreNamespaceName.Default, Tenant];

    void Establish()
    {
        _inbox = Substitute.For<IEventStore>();
        _inbox.GetNamespaces(Arg.Any<CancellationToken>()).Returns(_ => Namespaces);
        _tenantStore = new();
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Application, Arg.Is<EventStoreNamespaceName?>(_ => _ == EventStoreNamespaceName.Default)).Returns(_inbox);
        _client.GetEventStore(Application, Arg.Is<EventStoreNamespaceName?>(_ => _ == Tenant)).Returns(_tenantStore);
        _logger = new();
        _translator = new(_client, _logger);
        _issueId = IssueId.New();
        _correlationId = CorrelationId.New();
    }

    void Destroy() => _tenantStore.Dispose();

    /// <summary>
    /// Lets the reporter report the issue in the tenant namespace.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected Task Reported() => _tenantStore.EventLog.Append(_issueId, IssueEvents.Described(Reporters.Identity));

    /// <summary>
    /// Creates the context of a desk event in the inbox.
    /// </summary>
    /// <param name="sequenceNumber">The sequence number of the event in the inbox.</param>
    /// <returns>The context.</returns>
    protected EventContext InboxContext(ulong sequenceNumber = 0) => EventContext.From(
        Application,
        EventStoreNamespaceName.Default,
        EventType.Unknown,
        EventSourceType.Default,
        _issueId,
        EventStreamType.All,
        EventStreamId.Default,
        sequenceNumber,
        _correlationId,
        Occurred);

    /// <summary>
    /// Reads what is recorded on the issue in the tenant namespace.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadRecorded() =>
        _recorded = await _tenantStore.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, _issueId);
}
#endif
