// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Access.given;

/// <summary>
/// The state of one user's right to give feedback, built from what the desk decided, each decision happening one minute
/// after the one before unless a specification says when.
/// </summary>
public class a_feedback_giver : Specification
{
    /// <summary>
    /// When the first decision happens.
    /// </summary>
    protected static readonly DateTimeOffset Start = new(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);

    protected ReadModelScenario<FeedbackGiver> _scenario;
    protected FeedbackGiverId _id;
    ulong _sequenceNumber;

    void Establish()
    {
        _scenario = new();
        _id = FeedbackGiverId.For(AccessEvents.Giver);
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Gets when the decision with a sequence number happened.
    /// </summary>
    /// <param name="sequenceNumber">The sequence number, counting from zero.</param>
    /// <returns>When it happened.</returns>
    protected static DateTimeOffset At(ulong sequenceNumber) => Start.AddMinutes(sequenceNumber);

    /// <summary>
    /// Lets the desk decide, one minute after the decision before.
    /// </summary>
    /// <param name="decisions">The decisions, in the order they reach the application.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Decided(params object[] decisions)
    {
        var first = _sequenceNumber;
        return DecidedAt([.. decisions.Select((decision, index) => (decision, At(first + (ulong)index)))]);
    }

    /// <summary>
    /// Lets the desk decide at stated times, in the order the decisions reach the application.
    /// </summary>
    /// <param name="decisions">The decisions with when the desk made them.</param>
    /// <returns>An awaitable task.</returns>
    protected Task DecidedAt(params (object Decision, DateTimeOffset At)[] decisions) =>
        _scenario.ProcessAppendedEvents([.. decisions.Select(decision =>
        {
            var context = EventContext.From(
                EventStoreName.NotSet,
                EventStoreNamespaceName.NotSet,
                EventType.Unknown,
                EventSourceType.Default,
                _id,
                EventStreamType.All,
                EventStreamId.Default,
                _sequenceNumber++,
                Cratis.Execution.CorrelationId.New(),
                decision.At);
            return new AppendedEvent(context, decision.Decision);
        })]);
}
#endif
