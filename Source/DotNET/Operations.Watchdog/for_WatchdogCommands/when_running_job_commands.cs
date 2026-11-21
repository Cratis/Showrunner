// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Configuration;
using Cratis.Showrunner.Operations.given;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.Operations.Watchdog.for_WatchdogCommands;

public class when_running_job_commands : Specification
{
    FakeConfigMapStore _store;
    FakeRunnerCluster _runners;
    FakeAgentHarnessCluster _agents;
    StubHttpHandler _discord;
    StubHttpHandler _runnersDiscord;
    StubHttpHandler _volumesDiscord;
    int _runnersExit;
    int _agentsExit;
    int _volumesExit;
    int _notConfigured;
    int _clusterExit;

    WatchdogFactory Factory(Dictionary<string, string> environment, StubHttpHandler? handler = null)
    {
        environment["DISCORD_WEBHOOK_URL"] = "https://discord.example.com/api/webhooks/1/x";
        _discord = handler ?? StubHttpHandler.Answering(HttpStatusCode.NoContent);
        var volumes = new FakeVolumeCloud();
        volumes.Handles.Add("live");
        return new WatchdogFactory(
            WatchdogSettingsReader.FromEnvironment(environment, "operations"),
            new FakeClusterReader(),
            _store,
            new HttpClient(_discord),
            null,
            new ManualTimeProvider { AutoFireWithin = TimeSpan.FromSeconds(30) },
            NullLoggerFactory.Instance,
            null,
            _runners,
            _agents,
            volumes);
    }

    async Task Because()
    {
        _store = new FakeConfigMapStore();
        _runners = new FakeRunnerCluster();
        _agents = new FakeAgentHarnessCluster();

        // A runner that has held a job for ever: the runners job stops it and tells every channel.
        _runners.Runners.Add(new RunnerInfo("hung", "big", new DateTimeOffset(2026, 7, 1, 8, 0, 0, TimeSpan.Zero), "Running", true, "Cratis/Studio", "build"));
        _store.Values[("runner-watchdog-state", "busy")] = "{\"hung\":1}";
        var environment = new Dictionary<string, string> { ["RUNNER_NAMESPACE"] = "arc-runners", ["RUNNER_SCALE_SETS"] = "big", ["WATCHDOG_CHECKS"] = "pods,runners" };
        _runnersExit = await WatchdogCommands.Run(WatchdogCommand.Runners, Factory(environment), CancellationToken.None);
        _runnersDiscord = _discord;

        _agentsExit = await WatchdogCommands.Run(WatchdogCommand.AgentHarness, Factory(new Dictionary<string, string> { ["AGENT_NAMESPACE"] = "direct" }), CancellationToken.None);

        // The sweeper cannot run with no persistent volumes and no cloud: that is critical and must be told.
        _volumesExit = await WatchdogCommands.Run(
            WatchdogCommand.Volumes,
            Factory(new Dictionary<string, string> { ["UPCLOUD_USERNAME"] = "svc", ["UPCLOUD_PASSWORD"] = "pw" }, new StubHttpHandler(request => new HttpResponseMessage(request.RequestUri!.Host == "api.upcloud.com" ? HttpStatusCode.InternalServerError : HttpStatusCode.NoContent))),
            CancellationToken.None);
        _volumesDiscord = _discord;

        try
        {
            _notConfigured = await WatchdogCommands.Run(WatchdogCommand.Runners, Factory(new Dictionary<string, string> { ["WATCHDOG_CHECKS"] = "pods" }), CancellationToken.None);
        }
        catch (WatchdogConfigurationException)
        {
            _notConfigured = -1;
        }

        _clusterExit = await WatchdogCommands.Run(WatchdogCommand.Cluster, Factory(environment), CancellationToken.None);
    }

    [Fact] void should_stop_the_hung_job() => _runners.Deleted.ShouldContainOnly("hung");
    [Fact] void should_keep_the_alert_state_of_each_job_apart() => _store.Writes.Select(write => write.ConfigMap).Distinct().ShouldContain("cluster-health-watchdog-state-runners");
    [Fact] void should_not_put_a_jobs_findings_in_the_cluster_state() => _store.Writes.Where(write => write.ConfigMap == "cluster-health-watchdog-state").Any(write => write.Value.Contains("runners:")).ShouldBeFalse();
    [Fact] void should_run_the_job_through_the_shared_channels() => _runnersDiscord.Requests.Single().Body.ShouldContain("stopped runner hung");
    [Fact] void should_succeed_when_the_job_ran() => (_runnersExit, _agentsExit).ShouldEqual((0, 0));
    [Fact] void should_leave_the_agent_job_with_nothing_to_do_without_alerts() => _agents.Deleted.ShouldBeEmpty();
    [Fact] void should_not_run_a_job_in_the_cluster_pass() => _clusterExit.ShouldEqual(0);
    [Fact] void should_fail_when_the_job_is_not_configured() => _notConfigured.ShouldEqual(-1);
    [Fact] void should_succeed_when_the_alert_about_the_sweeper_was_delivered() => _volumesExit.ShouldEqual(0);
    [Fact] void should_name_the_sweeper_in_that_alert() => _volumesDiscord.Requests.Single(request => request.Uri.Host == "discord.example.com").Body.ShouldContain("volume sweeper cannot run");
}
#endif
