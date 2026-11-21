// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_no_heartbeat_is_configured : a_watchdog_run
{
    void Establish() => _checks.Add(StubCheck.Finding("check"));

    async Task Because() => _outcome = await CreateRun(withHeartbeat: false).Run(CancellationToken.None);

    [Fact] void should_succeed() => _outcome.Succeeded.ShouldBeTrue();
    [Fact] void should_not_ping() => _heartbeat.Pings.ShouldBeEmpty();
}
#endif
