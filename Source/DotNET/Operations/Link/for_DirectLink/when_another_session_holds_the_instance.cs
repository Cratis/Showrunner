// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_another_session_holds_the_instance : a_direct_link
{
    int _eventsBefore;

    async Task Because()
    {
        PublishReadiness();
        _endpointStatus["/ready"] = System.Net.HttpStatusCode.ServiceUnavailable;
        _direct.AutoAuthorize = true;
        await Pass();
        _eventsBefore = _direct.Events.Count;
        _direct.TakeOver("a-copied-container");
        _time.Advance(TimeSpan.FromSeconds(30));
        await Pass(Failing("pod:shop/web:CrashLoopBackOff"));
    }

    [Fact] void should_report_that_it_was_fenced() => Status.Connection.ShouldEqual(LinkConnection.Fenced);
    [Fact] void should_stop_delivering_events() => _direct.Events.Count.ShouldEqual(_eventsBefore);
    [Fact] void should_not_run_a_remedy_it_is_no_longer_entitled_to() => _deployments.Restarted.Count.ShouldEqual(1);
    [Fact] void should_say_why_in_its_status() => Status.Problems.ShouldNotBeEmpty();
}
#endif
