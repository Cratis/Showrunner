// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_AgentHarnessCheck;

public class when_the_pass_runs_out_of_time : an_agent_harness
{
    async Task Because()
    {
        _budget = TimeSpan.Zero;
        await Scan();
    }

    [Fact] void should_not_know_what_state_the_cluster_is_in() => _counts.Unknown.ShouldEqual(1);
    [Fact] void should_not_delete_anything() => _cluster.Deleted.ShouldBeEmpty();
    [Fact] void should_not_count_a_job_it_never_looked_at() => _counts.Considered.ShouldEqual(0);
}
#endif
