// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_the_watchdog_jobs_cannot_be_listed : a_watchdog_run
{
    void Establish()
    {
        _options = _options with { WatchdogCronJob = "cluster-health-watchdog" };
        _reader.Failing[nameof(IClusterReader.ListJobs)] = 403;
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_not_treat_an_unreadable_source_as_healthy() => Notification.Raised.Select(finding => finding.Key).ShouldContainOnly("watchdog:jobs-api");
}
#endif
