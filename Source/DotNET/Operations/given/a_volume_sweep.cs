// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Checks;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// A cluster with one volume that backs a PersistentVolume and one old volume nothing refers to, both provisioned by this cluster.
/// </summary>
public abstract class a_volume_sweep : a_check
{
    protected FakeVolumeCloud _cloud;
    protected VolumeSweepOptions _options = new() { DetachPoll = TimeSpan.FromSeconds(1) };

    protected override IWatchdogCheck Check => new VolumeSweepCheck(_cloud, _cloud, _options);

    protected static TimeSpan Hours(int hours) => TimeSpan.FromHours(hours);

    protected void AddVolume(string id, TimeSpan age, string title = "pvc-x", string? cluster = "mine") =>
        _cloud.Volumes.Add(FakeVolumeCloud.Volume(id, age, _time.Now, title, cluster));

    void Establish()
    {
        _cloud = new FakeVolumeCloud();
        _cloud.Handles.Add("live");
        _cloud.Volumes.Add(FakeVolumeCloud.Volume("live", Hours(100), _time.Now));
        _cloud.Volumes.Add(FakeVolumeCloud.Volume("orphan", Hours(3), _time.Now));
    }
}
#endif
