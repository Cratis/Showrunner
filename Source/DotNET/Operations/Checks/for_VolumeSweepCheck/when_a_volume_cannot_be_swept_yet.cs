// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_VolumeSweepCheck;

public class when_a_volume_cannot_be_swept_yet : a_volume_sweep
{
    IReadOnlyCollection<Alerts.Finding> _maintenance;
    IReadOnlyCollection<Alerts.Finding> _stuck;
    IReadOnlyCollection<Alerts.Finding> _failed;
    List<string> _actionsInMaintenance;

    async Task Because()
    {
        // A server in maintenance is retried on the next run.
        _cloud.Attached["orphan"] = [new CloudAttachment("server-1", false, "0:2")];
        await Run();
        _maintenance = _findings;
        _actionsInMaintenance = [.. _cloud.Actions];

        // Still attached after twelve tries.
        _cloud.Attached["orphan"] = [new CloudAttachment("server-1", true, "0:2")];
        _cloud.AttachedForAttempts = 99;
        await Run();
        _stuck = _findings;

        // One volume fails and the others are still swept.
        _cloud.AttachedForAttempts = 0;
        _cloud.Actions.Clear();
        AddVolume("second", Hours(4));
        _cloud.FailingDeletes.Add("orphan");
        await Run();
        _failed = _findings;
    }

    [Fact] void should_leave_a_volume_on_a_server_in_maintenance_without_detaching() => _actionsInMaintenance.ShouldBeEmpty();
    [Fact] void should_not_report_a_volume_it_left_for_the_next_run() => _maintenance.ShouldBeEmpty();
    [Fact] void should_not_report_a_volume_that_is_still_attached_after_the_tries() => _stuck.ShouldBeEmpty();
    [Fact] void should_report_a_volume_that_failed_to_delete() => _failed.Select(finding => finding.Key).ShouldContain("volume-sweeper:failed:orphan");
    [Fact] void should_still_sweep_the_others_when_one_fails() => _failed.Select(finding => finding.Key).ShouldContain("volume-sweeper:deleted");
    [Fact] void should_not_report_the_failure_body() => _failed.Single(finding => finding.Key == "volume-sweeper:failed:orphan").Text.ShouldNotContain("INTERNAL");
}
#endif
