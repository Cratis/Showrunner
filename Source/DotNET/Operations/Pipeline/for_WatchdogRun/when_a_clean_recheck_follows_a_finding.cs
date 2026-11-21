// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_clean_recheck_follows_a_finding : a_watchdog_run
{
    StubCheck _check;

    void Establish()
    {
        _check = StubCheck.FindingOnlyOnce("check", new Finding("pod:shop/web:CrashLoopBackOff", "shop/web crash looping", AlertSeverity.Critical));
        _checks.Add(_check);
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_look_a_second_time() => _check.Passes.ShouldEqual(2);
    [Fact] void should_treat_the_first_pass_as_resolved() => _channel.Delivered.ShouldBeEmpty();
    [Fact] void should_not_remember_a_first_seen_time() => _state.Current.FirstSeen.ShouldBeEmpty();
    [Fact] void should_not_remember_a_cooldown() => _state.Current.LastAlerted.ShouldBeEmpty();
    [Fact] void should_succeed() => _outcome.Succeeded.ShouldBeTrue();
}
#endif
