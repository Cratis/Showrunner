// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.ReadModels;
using Cratis.Chronicle.Transactions;
using Cratis.Execution;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.given;

/// <summary>
/// <see cref="TenantEntitlements"/> over read models that fold the events the operator's decisions became, answered by
/// a substituted <see cref="IReadModels"/>.
/// </summary>
public class some_entitlements : Specification
{
    protected Clock _clock;
    protected IReadModels _readModels;
    protected TenantEntitlements _entitlements;
    readonly List<(DateTimeOffset At, object Event)> _facts = [];

    void Establish()
    {
        _clock = new(Tenants.Now);
        _readModels = Substitute.For<IReadModels>();
        _entitlements = new(_readModels, Substitute.For<IUnitOfWorkManager>(), _clock);
    }

    /// <summary>
    /// Lets the operator's decisions and the product's facts happen one minute apart, in the order given, then makes the read
    /// models answer what they fold from them.
    /// </summary>
    /// <param name="facts">The facts.</param>
    protected void Given(params object[] facts)
    {
        _facts.AddRange(facts.Select((fact, index) => (Tenants.At(_facts.Count + index), fact)));
        Answer<SeatCapacity>();
        Answer<SeatRoster>();
        Answer<TenantStanding>();
        Answer<ModuleEntitlements>();
        Answer<Allowances>();
    }

    void Answer<T>()
        where T : class
    {
        var fold = Fold<T>();
        _readModels.GetInstanceById<T>(Arg.Any<ReadModelKey>(), Arg.Any<ReadModelSessionId>()).Returns(fold!);
    }

    T? Fold<T>()
        where T : class
    {
        T? current = null;
        foreach (var (at, fact) in _facts)
        {
            var context = EventContext.From(
                EventStoreName.NotSet,
                EventStoreNamespaceName.NotSet,
                EventType.Unknown,
                EventSourceType.Default,
                TenantStream.Id,
                EventStreamType.All,
                EventStreamId.Default,
                0,
                CorrelationId.New(),
                at);
            var reduce = typeof(T).GetMethods().FirstOrDefault(_ =>
                _.Name == "Reduce" && _.IsStatic && _.GetParameters()[0].ParameterType == fact.GetType());
            if (reduce is null)
            {
                continue;
            }

            var arguments = reduce.GetParameters().Length == 3 ? new[] { fact, current, context } : [fact, current];
            current = (T?)reduce.Invoke(null, arguments);
        }

        return current;
    }
}
#endif
