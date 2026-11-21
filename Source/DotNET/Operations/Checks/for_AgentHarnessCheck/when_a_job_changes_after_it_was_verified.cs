// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_AgentHarnessCheck;

public class when_a_job_changes_after_it_was_verified : an_agent_harness
{
    async Task Because()
    {
        // The last read of the Job, right before the delete, is the second.
        _cluster.OnJobRead = read =>
        {
            if (read == 1)
            {
                Mutate(job => job with { ResourceVersion = "late-change" });
            }
        };
        await Scan();
    }

    [Fact] void should_not_delete_it() => _cluster.Deleted.ShouldBeEmpty();
    [Fact] void should_protect_it() => _counts.Protected.ShouldEqual(1);
}
#endif
