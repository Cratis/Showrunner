// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_the_collection_deadline_has_passed : a_watchdog_run
{
    StubCheck _late;

    void Establish()
    {
        // The deadline is 240s minus the 100s delivery reserve; the first check uses all of it.
        _checks.Add(new StubCheck("slow", (_, _, _) =>
        {
            _time.Advance(TimeSpan.FromSeconds(141));
            return Task.FromResult<IReadOnlyCollection<Finding>>([new Finding("a", "a")]);
        }));
        _late = StubCheck.Finding("late");
        _checks.Add(_late);
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_not_start_the_late_check() => _late.Passes.ShouldEqual(0);
    [Fact] void should_say_which_check_did_not_run() => Notification.Raised.Select(finding => finding.Key).ShouldContain("watchdog:budget:late");
    [Fact] void should_still_alert_with_what_it_found() => Notification.Raised.Select(finding => finding.Key).ShouldContain("a");
}
#endif
