// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing.for_GrantLapsing.given;
using Cratis.Showrunner.Usher.Contracts.Access;

namespace Cratis.Showrunner.Usher.Access.Lapsing.for_GrantLapsing.when_lapsing_due_grants;

public class and_the_clock_reaches_the_end : grant_lapsing
{
    static readonly ValidUntil _validUntil = new(Now.AddHours(1));
    IEventStore _namespace;
    bool _lapsedBeforeTheEnd;

    void Establish()
    {
        _namespace = Substitute.For<IEventStore>();
        _namespace.EventLog.Returns(_eventStore.EventLog);
        _namespace.EventTypes.Returns(_eventStore.EventTypes);
        _namespace.ReadModels.GetInstances<GrantToLapse>().Returns([new GrantToLapse(_issueId, _validUntil)]);
    }

    async Task Because()
    {
        await HappenedOn(_issueId, new ReporterGrantedAccess(_validUntil));
        _time.Advance(TimeSpan.FromMinutes(59));
        await _lapsing.LapseDueGrants(_namespace);
        await ReadEventsOf(_issueId);
        _lapsedBeforeTheEnd = _events.Any(_ => _.Content is AccessGrantLapsed);
        _time.Advance(TimeSpan.FromMinutes(1));
        await _lapsing.LapseDueGrants(_namespace);
        await ReadEventsOf(_issueId);
    }

    [Fact] void should_not_lapse_it_before_the_end() => _lapsedBeforeTheEnd.ShouldBeFalse();
    [Fact] void should_lapse_it_at_the_end() => _events[^1].Content.ShouldEqual(new AccessGrantLapsed(_validUntil));
}
#endif
