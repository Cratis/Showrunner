// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_the_first_pass_is_too_slow_to_recheck : a_watchdog_run
{
    StubCheck _check;

    void Establish()
    {
        // 80s for the pass, then a pause, another 80s pass and the 100s delivery reserve would not fit in 240s.
        _check = new StubCheck("check", (_, _, _) =>
        {
            _time.Advance(TimeSpan.FromSeconds(80));
            return Task.FromResult<IReadOnlyCollection<Finding>>([new Finding("a", "a")]);
        });
        _checks.Add(_check);
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_not_look_again() => _check.Passes.ShouldEqual(1);
    [Fact] void should_alert_on_the_first_pass_rather_than_stay_silent() => Notification.Raised.Select(finding => finding.Key).ShouldContainOnly("a");
}
#endif
