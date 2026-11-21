// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_VolumeSweepCheck;

public class when_a_persistent_volume_appears_during_the_sweep : a_volume_sweep
{
    void Establish()
    {
        AddVolume("second", Hours(4));
        _cloud.LateHandles.Add("second");
    }

    async Task Because() => await Run();

    [Fact] void should_protect_a_volume_a_new_persistent_volume_now_uses() => _cloud.Actions.ShouldNotContain("delete second");
    [Fact] void should_still_delete_the_other_orphan() => _cloud.Actions.ShouldContain("delete orphan");
}
#endif
