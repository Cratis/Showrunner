// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Execution;
using Cratis.Showrunner.GuestList.given;

namespace Cratis.Showrunner.GuestList.Holding.for_SuspensionHoldings.when_reducing;

/// <summary>
/// The newest decision about a part wins whatever order the decisions arrive in: a suspension that is older than the resume
/// already recorded does not suspend the part again, and a newer one does.
/// </summary>
public class a_newer_resume_and_a_late_suspension : Specification
{
    static readonly PartPath Part = "Fleet/Reports";

    ReadModelScenario<SuspensionHoldings> _scenario;
    bool _afterLateSuspension;
    bool _afterNewerSuspension;
    ulong _sequenceNumber;

    void Establish() => _scenario = new();

    void Destroy() => _scenario.Dispose();

    async Task Because()
    {
        await Record((2, new PartResumeRecorded(SuspensionLevel.Tenant, Part)), (1, new PartSuspensionRecorded(SuspensionLevel.Tenant, Part)));
        _afterLateSuspension = _scenario.Instance!.Decisions.Single().IsSuspended;
        await Record((3, new PartSuspensionRecorded(SuspensionLevel.Tenant, Part)));
        _afterNewerSuspension = _scenario.Instance!.Decisions.Single().IsSuspended;
    }

    [Fact] void should_ignore_a_suspension_older_than_the_resume() => _afterLateSuspension.ShouldBeFalse();
    [Fact] void should_suspend_again_on_a_newer_suspension() => _afterNewerSuspension.ShouldBeTrue();
    [Fact] void should_date_the_decision_by_when_the_operator_decided() => _scenario.Instance!.Decisions.Single().Decided.ShouldEqual(Guests.At(3));

    Task Record(params (int Minute, object Fact)[] facts) =>
        _scenario.ProcessAppendedEvents([.. facts.Select(fact =>
        {
            var context = EventContext.From(
                EventStoreName.NotSet,
                EventStoreNamespaceName.NotSet,
                EventType.Unknown,
                EventSourceType.Default,
                new EventSourceId("tenant"),
                EventStreamType.All,
                EventStreamId.Default,
                _sequenceNumber++,
                CorrelationId.New(),
                Guests.At(fact.Minute));
            return new AppendedEvent(context, fact.Fact);
        })]);
}
#endif
