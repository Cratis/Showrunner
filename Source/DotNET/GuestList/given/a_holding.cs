// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Execution;

namespace Cratis.Showrunner.GuestList.given;

/// <summary>
/// The audiences held in one stream, built from facts that were decided at the times the specification gives, in the
/// order they arrive.
/// </summary>
public class a_holding : Specification
{
    protected ReadModelScenario<AudienceHoldings> _scenario;
    ulong _sequenceNumber;

    void Establish() => _scenario = new();

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Gets what is held at the end.
    /// </summary>
    protected AudienceHoldings Holdings => _scenario.Instance!;

    /// <summary>
    /// Lets the product record facts in a stream, in the order given, each as the operator decided it.
    /// </summary>
    /// <param name="stream">The stream the facts are recorded in.</param>
    /// <param name="facts">The facts, with the minute the operator decided each.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Recorded(EventSourceId stream, params (int Minute, object Fact)[] facts) =>
        _scenario.ProcessAppendedEvents([.. facts.Select(fact =>
        {
            var context = EventContext.From(
                EventStoreName.NotSet,
                EventStoreNamespaceName.NotSet,
                EventType.Unknown,
                EventSourceType.Default,
                stream,
                EventStreamType.All,
                EventStreamId.Default,
                _sequenceNumber++,
                CorrelationId.New(),
                Guests.At(fact.Minute));
            return new AppendedEvent(context, fact.Fact);
        })]);
}
#endif
