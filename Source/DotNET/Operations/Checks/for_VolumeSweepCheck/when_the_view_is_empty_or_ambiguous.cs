// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_VolumeSweepCheck;

public class when_the_view_is_empty_or_ambiguous : a_volume_sweep
{
    readonly Dictionary<string, IReadOnlyCollection<Finding>> _outcomes = [];

    async Task Because()
    {
        _cloud.Handles.Clear();
        await Run();
        _outcomes["no persistent volumes"] = _findings;

        _cloud.Handles.Add("live");
        _cloud.Volumes.Clear();
        await Run();
        _outcomes["no storage"] = _findings;

        _cloud.Volumes.Add(FakeVolumeCloud.Volume("live", Hours(100), _time.Now));
        _cloud.Volumes.Add(FakeVolumeCloud.Volume("live-2", Hours(100), _time.Now, cluster: "other"));
        _cloud.Handles.Add("live-2");
        _cloud.Volumes.Add(FakeVolumeCloud.Volume("orphan", Hours(5), _time.Now));
        await Run();
        _outcomes["two cluster ids"] = _findings;

        _cloud.Handles.Clear();
        _cloud.Handles.Add("not-in-the-cloud");
        await Run();
        _outcomes["no cluster id"] = _findings;

        _cloud.Handles.Add("live");
        _cloud.ListingFails = true;
        await Run();
        _outcomes["the cloud fails"] = _findings;
    }

    [Fact] void should_never_report_a_pass_for_any_of_them() => _outcomes.Values.All(findings => findings.Single().Key == "volume-sweeper:cannot-run" && findings.Single().Severity == AlertSeverity.Critical).ShouldBeTrue();
    [Fact] void should_not_treat_every_volume_as_an_orphan_without_persistent_volumes() => _outcomes["no persistent volumes"].Single().Text.ShouldContain("refusing to treat every volume as an orphan");
    [Fact] void should_say_the_cloud_has_nothing() => _outcomes["no storage"].Single().Text.ShouldContain("the cloud reports no storage at all");
    [Fact] void should_name_the_ambiguous_cluster_ids() => _outcomes["two cluster ids"].Single().Text.ShouldContain("found [mine, other]");
    [Fact] void should_say_when_there_is_no_cluster_id() => _outcomes["no cluster id"].Single().Text.ShouldContain("found []");
    [Fact] void should_not_delete_anything() => _cloud.Actions.ShouldBeEmpty();
    [Fact] void should_not_leak_a_cloud_failure_detail() => _outcomes["the cloud fails"].Single().Text.ShouldContain("GET storage/normal: HTTP 500");
}
#endif
