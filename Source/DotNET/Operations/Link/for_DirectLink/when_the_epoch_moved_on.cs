// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_the_epoch_moved_on : a_direct_link
{
    int _eventsBefore;

    async Task Because()
    {
        await Pass();
        _eventsBefore = _direct.Events.Count;

        // Another session took over and the lease lapsed: this watchdog's saved epoch is now stale.
        _direct.TakeOver("another");
        _time.Advance(TimeSpan.FromMinutes(5));
        await Pass(Failing("pod:shop/web:CrashLoopBackOff"));
    }

    [Fact] void should_claim_again_as_a_takeover_with_the_new_epoch() => Identity.Epoch.ShouldEqual(_direct.Epoch);
    [Fact] void should_be_connected_again() => Status.Connection.ShouldEqual(LinkConnection.Connected);
    [Fact] void should_deliver_what_it_observed() => _direct.Events.Count.ShouldBeGreaterThan(_eventsBefore);
}
#endif
