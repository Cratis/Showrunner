// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_the_configuration_lapses_while_direct_is_unreachable : a_direct_link
{
    DirectLink _link;

    async Task Because()
    {
        PublishReadiness();
        await Pass();
        _endpoints.Requests.Clear();
        _direct.Unreachable = true;
        _time.Advance(TimeSpan.FromHours(2));
        _link = await Pass(Failing("pod:shop/web:CrashLoopBackOff"));
    }

    [Fact] void should_stop_acting_on_the_lapsed_grant() => _link.ActiveConfiguration.ShouldBeNull();
    [Fact] void should_not_run_the_checks_direct_defined() => _endpoints.Requests.ShouldBeEmpty();
    [Fact] void should_say_so() => Status.Problems.ShouldContain(problem => problem.Contains("lapsed"));
    [Fact] void should_keep_observing_with_its_local_configuration() => Status.OutboxDepth.ShouldBeGreaterThan(0);
}
#endif
