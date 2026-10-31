// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Integration.given;

namespace Cratis.Showrunner.BoxOffice.Integration.for_MongoMeteringStore;

public class when_adding_at_the_same_time : a_metering_store
{
    bool[] _results;
    MeteringDay? _day;

    async Task Because()
    {
        _results = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => _store.Add(_tenant, "compute", Reading(30), 5UL, false)));
        _day = await _store.Get(_tenant, new DateOnly(2026, 10, 30));
    }

    [Fact] void should_add_it_once() => _results.Count(_ => _).ShouldEqual(1);
    [Fact] void should_total_it_once() => _day!.Quantities[new MeterId("compute")].Value.ShouldEqual(30L);
}
