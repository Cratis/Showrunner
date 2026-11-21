// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_recheck_still_finds_something_different : a_watchdog_run
{
    void Establish() => _checks.Add(new StubCheck("check", (pass, _, _) => Task.FromResult<IReadOnlyCollection<Finding>>(
        pass == 0
            ? [new Finding("a", "first a"), new Finding("b", "first b")]
            : [new Finding("a", "second a"), new Finding("c", "second c")])));

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_alert_on_both_passes() => Notification.Raised.Select(finding => finding.Key).ShouldContainOnly("a", "b", "c");
    [Fact] void should_use_the_text_of_the_latest_pass() => Notification.Raised.First(finding => finding.Key == "a").Text.ShouldContain("second a");
    [Fact] void should_keep_the_text_of_a_finding_only_the_first_pass_saw() => Notification.Raised.First(finding => finding.Key == "b").Text.ShouldContain("first b");
}
#endif
