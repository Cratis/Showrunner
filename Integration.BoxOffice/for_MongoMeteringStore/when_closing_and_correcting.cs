// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Integration.given;

namespace Cratis.Showrunner.BoxOffice.Integration.for_MongoMeteringStore;

public class when_closing_and_correcting : a_metering_store
{
    IReadOnlyList<MeteringDay> _openBefore;
    IReadOnlyList<MeteringDay> _openAfter;
    IReadOnlyList<MeteringDay> _beforeALateEvent;
    IReadOnlyList<MeteringDay> _afterALateEvent;
    IReadOnlyList<MeteringDay> _afterReportingAStaleCorrection;
    IReadOnlyList<MeteringDay> _afterReportingTheCorrection;
    IReadOnlyList<MeteringDay> _otherTenant;

    async Task Because()
    {
        await _store.Add(_tenant, "compute", Reading(30), 5UL, false);
        _openBefore = await _store.GetOpen(_tenant, new DateOnly(2026, 10, 30));
        _otherTenant = await _store.GetOpen("other", new DateOnly(2026, 10, 30));

        await _store.Close(_tenant, new DateOnly(2026, 10, 30));
        _openAfter = await _store.GetOpen(_tenant, new DateOnly(2026, 10, 30));
        _beforeALateEvent = await _store.GetAwaitingCorrection(_tenant);

        await _store.Add(_tenant, "compute", Reading(20), 9UL, true);
        _afterALateEvent = await _store.GetAwaitingCorrection(_tenant);

        await _store.Add(_tenant, "compute", Reading(1), 12UL, true);
        await _store.Corrected(_tenant, new DateOnly(2026, 10, 30), 9UL);
        _afterReportingAStaleCorrection = await _store.GetAwaitingCorrection(_tenant);

        await _store.Corrected(_tenant, new DateOnly(2026, 10, 30), 12UL);
        _afterReportingTheCorrection = await _store.GetAwaitingCorrection(_tenant);
    }

    [Fact] void should_find_the_open_day() => _openBefore.Count.ShouldEqual(1);
    [Fact] void should_not_find_the_day_of_another_tenant() => _otherTenant.Count.ShouldEqual(0);
    [Fact] void should_not_find_a_closed_day_as_open() => _openAfter.Count.ShouldEqual(0);
    [Fact] void should_not_have_a_correction_before_a_late_event() => _beforeALateEvent.Count.ShouldEqual(0);
    [Fact] void should_have_a_correction_after_a_late_event() => _afterALateEvent.Single().CorrectionDue!.Value.ShouldEqual(9UL);
    [Fact] void should_carry_the_corrected_total() => _afterALateEvent.Single().Quantities[new MeterId("compute")].Value.ShouldEqual(50L);
    [Fact] void should_keep_waiting_when_more_arrived_than_was_reported() => _afterReportingAStaleCorrection.Single().CorrectionDue!.Value.ShouldEqual(12UL);
    [Fact] void should_have_nothing_left_to_correct_once_reported() => _afterReportingTheCorrection.Count.ShouldEqual(0);
}
