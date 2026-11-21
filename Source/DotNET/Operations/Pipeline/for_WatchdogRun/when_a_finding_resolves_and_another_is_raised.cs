// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_finding_resolves_and_another_is_raised : a_watchdog_run
{
    void Establish()
    {
        FindsEveryPass(new Finding("pod:shop/web:Pending", "shop/web has been Pending"));
        _state.Current = StateWith("node:n1:NotReady", TimeSpan.FromHours(1));
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_tell_the_channels_what_resolved() => Notification.Resolved.ShouldContainOnly("node:n1:NotReady");
    [Fact] void should_alert_the_new_finding() => Notification.Raised.Select(finding => finding.Key).ShouldContainOnly("pod:shop/web:Pending");
    [Fact] void should_forget_the_resolved_finding() => _state.Current.LastAlerted.Keys.ShouldContainOnly("pod:shop/web:Pending");
}
#endif
