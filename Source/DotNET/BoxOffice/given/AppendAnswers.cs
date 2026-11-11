// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Castle.DynamicProxy;
using Cratis.Chronicle;
using Cratis.Chronicle.Events;
using Cratis.Chronicle.EventSequences;
using Cratis.Execution;

namespace Cratis.Showrunner.BoxOffice.given;

/// <summary>
/// Event stores that answer a once-only append the way a kernel that refuses it does, for the outcomes an in-process
/// event store cannot produce.
/// </summary>
public static class AppendAnswers
{
    static readonly ProxyGenerator Generator = new();

    /// <summary>
    /// Gets an append the event log did not take: it failed and the event is not in the log.
    /// </summary>
    public static AppendResult Rejected { get; } = new() { Errors = [new AppendError("rejected")] };

    /// <summary>
    /// Gets an append that failed because the idempotency key was used for something else.
    /// </summary>
    public static AppendResult Conflicting { get; } = new()
    {
        Errors = [new AppendError("conflict")],
        IdempotencyConflicts = [new IdempotencyKeyConflict(default!, default!, default!, default!, default)]
    };

    /// <summary>
    /// Gets an append that failed and found the event already in the log.
    /// </summary>
    public static AppendResult AlreadyAppended { get; } = new() { Errors = [new AppendError("repeated")], Outcome = AppendOutcome.AlreadyAppended };

    /// <summary>
    /// Wraps an event store so that once-only appends to its event log answer a given result and every read is the store's own.
    /// </summary>
    /// <param name="real">The event store to read from.</param>
    /// <param name="answer">What every once-only append to the event log answers.</param>
    /// <returns>The event store.</returns>
    public static IEventStore Answering(IEventStore real, AppendResult answer) => Wrap(real, answer, null);

    /// <summary>
    /// Wraps an event store so that once-only appends to its outbox answer a given result, and everything else is the store's own.
    /// </summary>
    /// <param name="real">The event store to append to and read from.</param>
    /// <param name="answer">What every once-only append to the outbox answers.</param>
    /// <returns>The event store.</returns>
    public static IEventStore AnsweringForTheOutbox(IEventStore real, AppendResult answer) => Wrap(real, null, answer);

    static IEventStore Wrap(IEventStore real, AppendResult? logAnswer, AppendResult? outboxAnswer)
    {
        var store = Substitute.For<IEventStore>();
        store.EventLog.Returns(Answering(real.EventLog, logAnswer));
        store.EventTypes.Returns(real.EventTypes);
        store.Namespace.Returns(real.Namespace);
        store.GetEventSequence(EventSequenceId.Outbox).Returns(Answering(real.GetEventSequence(EventSequenceId.Outbox), outboxAnswer));
        return store;
    }

    static T Answering<T>(T real, AppendResult? answer)
        where T : class, IEventSequence
    {
        // AppendOnce is a default interface method, which NSubstitute cannot configure, so the real sequence is proxied instead.
        return Generator.CreateInterfaceProxyWithTarget(real, new AnswerOnce(answer));
    }

    sealed class AnswerOnce(AppendResult? answer) : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            if (answer is not null && invocation.Method.Name == nameof(IEventSequence.AppendOnce))
            {
                invocation.ReturnValue = Task.FromResult(answer);
                return;
            }

            invocation.Proceed();
        }
    }
}
#endif
