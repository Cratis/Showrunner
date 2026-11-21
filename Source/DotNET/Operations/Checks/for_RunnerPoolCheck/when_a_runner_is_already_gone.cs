// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_RunnerPoolCheck;

public class when_a_runner_is_already_gone : a_runner_pool
{
    Exception _serverError;
    Exception _pendingGone;

    async Task Because()
    {
        _cluster.Runners.Add(Runner("hung", job: "x"));
        FirstSeen("hung", 100);
        _cluster.DeleteFailsWith = 404;
        await Run();

        _cluster.DeleteFailsWith = 500;
        _serverError = await Record.ExceptionAsync(() => Run());

        _cluster.Runners.Clear();
        _cluster.Runners.Add(Runner("stuck", ageMinutes: 45, phase: "Pending"));
        _cluster.DeleteFailsWith = 404;
        _pendingGone = await Record.ExceptionAsync(() => Run());
    }

    [Fact] void should_not_fail_for_a_runner_that_is_gone() => _findings.ShouldNotBeNull();
    [Fact] void should_fail_for_a_server_error() => _serverError.ShouldBeOfExactType<ClusterReadException>();
    [Fact] void should_not_fail_for_a_pending_runner_that_is_gone() => _pendingGone.ShouldBeNull();
}
#endif
