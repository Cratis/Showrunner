// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_VolumeSweepCheck;

public class when_the_sweep_is_a_dry_run : a_volume_sweep
{
    void Establish() => _options = _options with { DryRun = true };

    async Task Because() => await Run();

    [Fact] void should_delete_nothing() => _cloud.Actions.ShouldBeEmpty();
    [Fact] void should_say_what_it_would_delete() => _findings.Single().Text.ShouldEqual("volume sweeper would delete 1 orphaned volume(s): orphan (10 GB)");
}
#endif
