// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_VolumeSweepCheck;

public class when_too_many_volumes_are_orphaned : a_volume_sweep
{
    IReadOnlyCollection<Finding> _eleven;
    int _actionsWithEleven;
    IReadOnlyCollection<Finding> _ten;

    async Task Because()
    {
        for (var index = 0; index < 10; index++)
        {
            AddVolume($"extra-{index}", Hours(5));
        }

        await Run();
        _eleven = _findings;
        _actionsWithEleven = _cloud.Actions.Count;

        _cloud.Volumes.RemoveAt(_cloud.Volumes.Count - 1);
        await Run();
        _ten = _findings;
    }

    [Fact] void should_trip_the_fuse() => _eleven.Single().Text.ShouldEqual("volume sweeper fuse: 11 orphaned volumes exceeds the 10 limit; acting on none - investigate");
    [Fact] void should_act_on_none_of_them() => _actionsWithEleven.ShouldEqual(0);
    [Fact] void should_act_on_exactly_the_limit() => _ten.Single().Text.ShouldContain("deleted 10 orphaned volume(s)");
}
#endif
