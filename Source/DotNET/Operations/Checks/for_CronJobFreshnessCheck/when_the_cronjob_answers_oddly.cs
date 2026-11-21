// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_CronJobFreshnessCheck;

public class when_the_cronjob_answers_oddly : a_check
{
    readonly Dictionary<string, IEnumerable<string>> _outcomes = [];

    protected override IWatchdogCheck Check => new CronJobFreshnessCheck(_reader, [new CronJobExpectation("ops", "db-backups", TimeSpan.FromHours(3))]);

    async Task Because()
    {
        foreach (var status in new int?[] { 403, 404, 500, null })
        {
            _reader.Failing.Clear();
            _reader.Failing[nameof(IClusterReader.GetCronJob)] = status;
            await Run();
            _outcomes[$"status {status}"] = Keys.ToList();
        }

        _reader.Failing.Clear();
        _reader.CronJobs["db-backups"] = new CronJobInfo("db-backups", null, null);
        await Run();
        _outcomes["no time at all"] = Keys.ToList();

        _reader.CronJobs["db-backups"] = new CronJobInfo("db-backups", _time.Now.AddMinutes(-30), null);
        await Run();
        _outcomes["young and never succeeded"] = Keys.ToList();
    }

    [Fact] void should_not_treat_a_forbidden_cronjob_as_healthy() => _outcomes["status 403"].ShouldContainOnly("db-backups:api");
    [Fact] void should_not_treat_a_missing_cronjob_as_healthy() => _outcomes["status 404"].ShouldContainOnly("db-backups:api");
    [Fact] void should_not_treat_a_server_error_as_healthy() => _outcomes["status 500"].ShouldContainOnly("db-backups:api");
    [Fact] void should_not_treat_a_failure_without_a_status_as_healthy() => _outcomes["status "].ShouldContainOnly("db-backups:api");
    [Fact] void should_not_treat_a_cronjob_without_any_time_as_healthy() => _outcomes["no time at all"].ShouldContainOnly("db-backups:api");
    [Fact] void should_give_a_young_cronjob_that_never_succeeded_its_grace() => _outcomes["young and never succeeded"].ShouldBeEmpty();
}
#endif
