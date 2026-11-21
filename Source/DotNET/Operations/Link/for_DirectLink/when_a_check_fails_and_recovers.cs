// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_a_check_fails_and_recovers : a_direct_link
{
    const string Key = "pod:shop/web:ImagePullBackOff";

    IReadOnlyCollection<string> _openWhileFailing;

    async Task Because()
    {
        await Pass(Failing(Key));
        await Pass(Failing(Key));
        await Pass(Failing(Key));
        _openWhileFailing = _direct.OpenAlerts;
        await Pass();
    }

    [Fact] void should_count_consecutive_failures_on_the_first_two() => Sent<CheckFailed>().Select(failed => failed.ConsecutiveFailures).ShouldEqual([1, 2]);
    [Fact] void should_not_repeat_the_fact_on_every_further_pass() => Sent<CheckFailed>().Count().ShouldEqual(2);
    [Fact] void should_hold_an_alert_from_the_second_failure() => _openWhileFailing.ShouldContain(Key);
    [Fact] void should_report_the_recovery() => Sent<CheckRecovered>().Single().Check.ShouldEqual(Key);
    [Fact] void should_resolve_the_alert() => _direct.OpenAlerts.ShouldBeEmpty();
    [Fact] void should_point_the_recovery_at_the_failure_it_resolves() => _direct.Events.Single(envelope => envelope.Type == WatchdogEventTypes.CheckRecovered).CausationId.ShouldEqual(_direct.Events.Last(envelope => envelope.Type == WatchdogEventTypes.CheckFailed).EventId);
}
#endif
