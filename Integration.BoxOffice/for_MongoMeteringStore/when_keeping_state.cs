// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Integration.given;

namespace Cratis.Showrunner.BoxOffice.Integration.for_MongoMeteringStore;

public class when_keeping_state : a_metering_store
{
    static readonly DateTimeOffset _now = new(2026, 10, 31, 6, 0, 0, TimeSpan.Zero);
    EventSequenceNumber _cursorBefore;
    EventSequenceNumber _cursorAfter;
    bool _firstHolder;
    bool _secondHolder;
    bool _firstHolderAgain;
    bool _secondHolderAfterTheLease;

    async Task Because()
    {
        _cursorBefore = await _store.GetCursor(_tenant);
        await _store.SetCursor(_tenant, 42UL);
        _cursorAfter = await _store.GetCursor(_tenant);

        _firstHolder = await _store.TryHold("one", TimeSpan.FromMinutes(2), _now);
        _secondHolder = await _store.TryHold("two", TimeSpan.FromMinutes(2), _now.AddSeconds(30));
        _firstHolderAgain = await _store.TryHold("one", TimeSpan.FromMinutes(2), _now.AddSeconds(60));
        _secondHolderAfterTheLease = await _store.TryHold("two", TimeSpan.FromMinutes(2), _now.AddMinutes(10));
    }

    [Fact] void should_start_reading_at_the_first_event() => _cursorBefore.Value.ShouldEqual(0UL);
    [Fact] void should_remember_how_far_it_has_read() => _cursorAfter.Value.ShouldEqual(42UL);
    [Fact] void should_let_the_first_instance_measure() => _firstHolder.ShouldBeTrue();
    [Fact] void should_not_let_another_instance_measure_meanwhile() => _secondHolder.ShouldBeFalse();
    [Fact] void should_let_the_holder_renew() => _firstHolderAgain.ShouldBeTrue();
    [Fact] void should_let_another_instance_take_over_when_the_lease_ran_out() => _secondHolderAfterTheLease.ShouldBeTrue();
}
