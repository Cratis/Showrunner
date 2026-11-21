// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_AgentHarnessCheck;

public class when_the_secret_comes_back_while_the_evidence_is_read : an_agent_harness
{
    async Task Because()
    {
        // The Secret was missing when the evidence was gathered and restored during the second look; the worker started.
        _cluster.OnSecretRead = read =>
        {
            if (read == 1)
            {
                _cluster.SecretPresent = true;
                MutatePod(pod => pod with { Phase = "Running", ResourceVersion = "started" });
            }
        };
        await Scan();
    }

    [Fact] void should_protect_the_worker_that_started() => (_counts.Protected, _cluster.Deleted.Count).ShouldEqual((1, 0));
    [Fact] void should_have_fenced_the_pods_again_before_any_delete() => _cluster.PodLists.ShouldEqual(2);
}
#endif
