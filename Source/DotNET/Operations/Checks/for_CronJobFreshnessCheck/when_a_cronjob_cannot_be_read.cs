// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_CronJobFreshnessCheck;

public class when_a_cronjob_cannot_be_read : a_check
{
    protected override IWatchdogCheck Check => new CronJobFreshnessCheck(_reader, [new CronJobExpectation("ops", "db-backups", TimeSpan.FromHours(3), Optional: true)]);

    void Establish() => _reader.Failing[nameof(IClusterReader.GetCronJob)] = 403;

    async Task Because() => await Run();

    [Fact] void should_not_treat_an_unreadable_source_as_healthy_even_when_the_cronjob_is_optional() => Keys.ShouldContainOnly("db-backups:api");
}
#endif
