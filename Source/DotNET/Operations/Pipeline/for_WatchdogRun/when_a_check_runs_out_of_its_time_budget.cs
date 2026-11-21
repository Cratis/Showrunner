// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_check_runs_out_of_its_time_budget : a_watchdog_run
{
    TaskCompletionSource _started;

    void Establish()
    {
        _started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _checks.Add(new StubCheck("hung", async (pass, _, cancellationToken) =>
        {
            if (pass == 0)
            {
                _started.SetResult();
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }

            return [];
        }));
        _checks.Add(StubCheck.Finding("other", new Finding("pod:shop/web:Pending", "shop/web has been Pending")));
    }

    async Task Because()
    {
        var run = CreateRun().Run(CancellationToken.None);
        await _started.Task;
        _time.Advance(_options.CheckBudget + TimeSpan.FromSeconds(1));
        _outcome = await run;
    }

    [Fact] void should_not_abort_the_run() => _outcome.Succeeded.ShouldBeTrue();
    [Fact] void should_report_the_hung_check_as_a_finding() => Notification.Raised.First(finding => finding.Key == "watchdog:check:hung").Text.ShouldContain("did not finish within 60s");
    [Fact] void should_still_run_the_next_check() => Notification.Raised.Select(finding => finding.Key).ShouldContain("pod:shop/web:Pending");
}
#endif
