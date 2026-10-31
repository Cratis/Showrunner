// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Execution;

namespace Cratis.Showrunner.BoxOffice.given;

/// <summary>
/// A read model folded from the events of the tenant's stream, each of which carries the time the operator decided it.
/// </summary>
/// <typeparam name="TReadModel">The read model.</typeparam>
public class a_tenant_read_model<TReadModel> : Specification
    where TReadModel : class
{
    protected ReadModelScenario<TReadModel> _scenario;
    ulong _sequenceNumber;

    void Establish() => _scenario = new();

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Lets events happen one minute apart, after any that happened before.
    /// </summary>
    /// <param name="events">The events, in the order they arrive.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Happened(params object[] events) => Arrived([.. events.Select((@event, index) => (Tenants.At((int)_sequenceNumber + index), @event))]);

    /// <summary>
    /// Lets events arrive in the order given, each having happened at its own time.
    /// </summary>
    /// <param name="events">The events and when the operator decided each, in the order they arrive.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Arrived(params (DateTimeOffset At, object Event)[] events) =>
        _scenario.ProcessAppendedEvents([.. events.Select(_ => new AppendedEvent(
            EventContext.From(
                EventStoreName.NotSet,
                EventStoreNamespaceName.NotSet,
                EventType.Unknown,
                EventSourceType.Default,
                TenantStream.Id,
                EventStreamType.All,
                EventStreamId.Default,
                _sequenceNumber++,
                CorrelationId.New(),
                _.At),
            _.Event))]);
}
#endif
