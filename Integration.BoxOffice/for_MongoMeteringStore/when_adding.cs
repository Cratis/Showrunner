// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Integration.given;

namespace Cratis.Showrunner.BoxOffice.Integration.for_MongoMeteringStore;

public class when_adding : a_metering_store
{
    bool _first;
    bool _again;
    bool _next;
    bool _otherMeter;
    MeteringDay? _result;

    async Task Because()
    {
        _first = await _store.Add(_tenant, "compute", Reading(30), 5UL, false);
        _again = await _store.Add(_tenant, "compute", Reading(30), 5UL, false);
        _next = await _store.Add(_tenant, "compute", Reading(15), 7UL, false);
        _otherMeter = await _store.Add(_tenant, "storage", Reading(2), 7UL, false);
        _result = await _store.Get(_tenant, _day);
    }

    [Fact] void should_add_the_first_event() => _first.ShouldBeTrue();
    [Fact] void should_not_add_the_same_event_twice() => _again.ShouldBeFalse();
    [Fact] void should_add_the_next_event() => _next.ShouldBeTrue();
    [Fact] void should_add_the_same_event_to_another_meter() => _otherMeter.ShouldBeTrue();
    [Fact] void should_total_each_event_once() => _result!.Quantities[new MeterId("compute")].Value.ShouldEqual(45L);
    [Fact] void should_total_each_meter() => _result!.Quantities[new MeterId("storage")].Value.ShouldEqual(2L);
    [Fact] void should_keep_the_organization() => _result!.Organization.ShouldEqual(_organization);
    [Fact] void should_leave_the_day_open() => _result!.IsClosed.ShouldBeFalse();
}
