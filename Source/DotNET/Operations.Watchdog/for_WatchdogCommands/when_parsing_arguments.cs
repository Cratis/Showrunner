// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Watchdog.for_WatchdogCommands;

public class when_parsing_arguments : Specification
{
    WatchdogCommand _cluster;
    WatchdogCommand _testAlert;
    WatchdogCommand _none;
    WatchdogCommand _unknown;
    WatchdogCommand _extra;

    void Because()
    {
        _cluster = WatchdogCommands.Parse(["cluster"]);
        _testAlert = WatchdogCommands.Parse(["test-alert"]);
        _none = WatchdogCommands.Parse([]);
        _unknown = WatchdogCommands.Parse(["restart"]);
        _extra = WatchdogCommands.Parse(["cluster", "--now"]);
    }

    [Fact] void should_run_the_cluster_pass() => _cluster.ShouldEqual(WatchdogCommand.Cluster);
    [Fact] void should_name_every_job_kind() =>
        new[] { "runners", "agent-harness", "volumes", "probe", "probe-external" }.Select(name => WatchdogCommands.Parse([name]))
            .ShouldContainOnly(WatchdogCommand.Runners, WatchdogCommand.AgentHarness, WatchdogCommand.Volumes, WatchdogCommand.Probe, WatchdogCommand.ProbeExternal);
    [Fact] void should_list_every_command_in_the_usage() => new[] { "cluster", "test-alert", "runners", "agent-harness", "volumes", "probe", "probe-external" }.All(WatchdogCommands.Usage.Contains).ShouldBeTrue();
    [Fact] void should_run_the_delivery_test() => _testAlert.ShouldEqual(WatchdogCommand.TestAlert);
    [Fact] void should_not_guess_a_command_from_nothing() => _none.ShouldEqual(WatchdogCommand.Unknown);
    [Fact] void should_not_guess_an_unknown_command() => _unknown.ShouldEqual(WatchdogCommand.Unknown);
    [Fact] void should_not_accept_trailing_arguments() => _extra.ShouldEqual(WatchdogCommand.Unknown);
    [Fact] void should_exit_with_two_for_usage_errors() => WatchdogCommands.UsageExitCode.ShouldEqual(2);
}
#endif
