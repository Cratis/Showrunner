// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_check_throws : a_watchdog_run
{
    void Establish()
    {
        _checks.Add(new StubCheck("broken", (_, _, _) => throw new InvalidOperationException("boom with https://user:pass@example.com/path password=hunter2")));
        _checks.Add(StubCheck.Finding("healthy-looking", new Finding("pod:shop/web:Pending", "shop/web has been Pending")));
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_not_abort_the_run() => _outcome.Succeeded.ShouldBeTrue();
    [Fact] void should_report_the_failing_check_as_its_own_finding() => Notification.Raised.Select(finding => finding.Key).ShouldContain("watchdog:check:broken");
    [Fact] void should_keep_the_findings_of_the_other_checks() => Notification.Raised.Select(finding => finding.Key).ShouldContain("pod:shop/web:Pending");
    [Fact] void should_redact_urls_from_the_failure() => Notification.Raised.First(finding => finding.Key == "watchdog:check:broken").Text.ShouldNotContain("example.com");
    [Fact] void should_redact_credentials_from_the_failure() => Notification.Raised.First(finding => finding.Key == "watchdog:check:broken").Text.ShouldNotContain("hunter2");
}
#endif
