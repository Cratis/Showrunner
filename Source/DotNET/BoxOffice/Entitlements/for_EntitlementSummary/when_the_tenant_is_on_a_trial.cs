// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_EntitlementSummary;

public class when_the_tenant_is_on_a_trial : a_summary_over_entitlements
{
    EntitlementSummary _running;
    EntitlementSummary _closed;

    async Task Because()
    {
        Given(new OperatorOpenedTrial(new(Tenants.Day(3)), 5));
        _running = await Summarize();

        _clock.MoveTo(Tenants.Day(4));
        _closed = await Summarize();
    }

    [Fact] void should_be_a_trial() => _running.IsTrial.ShouldBeTrue();
    [Fact] void should_tell_the_last_day_of_the_trial() => _running.TrialEnds.ShouldEqual(Tenants.Day(3));
    [Fact] void should_have_the_seats_of_the_trial() => _running.Seats.ShouldEqual(5);
    [Fact] void should_not_be_read_only_while_it_runs() => _running.ReadOnlyReason.ShouldEqual(ReadOnlyReason.None);
    [Fact] void should_still_be_a_trial_when_it_has_closed() => _closed.IsTrial.ShouldBeTrue();
    [Fact] void should_be_read_only_because_the_trial_closed() => _closed.ReadOnlyReason.ShouldEqual(ReadOnlyReason.TrialClosed);
    [Fact] void should_have_no_seats_when_it_has_closed() => _closed.Seats.ShouldEqual(0);
}
#endif
