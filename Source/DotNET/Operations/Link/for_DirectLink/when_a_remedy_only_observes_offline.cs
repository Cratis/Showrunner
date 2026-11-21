// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_a_remedy_only_observes_offline : a_direct_link
{
    OutboxSnapshot _outbox;

    async Task Because()
    {
        PublishReadiness(RemedyPolicyMode.Review, RemedyPolicyMode.Never, true);
        _endpointStatus["/ready"] = HttpStatusCode.ServiceUnavailable;
        await Pass();
        _direct.Unreachable = true;
        _time.Advance(TimeSpan.FromSeconds(30));
        await Pass();
        _outbox = await _store.LoadOutbox(CancellationToken.None);
    }

    [Fact] void should_not_run_anything() => _deployments.Restarted.ShouldBeEmpty();
    [Fact] void should_queue_the_observation_for_when_direct_is_back() => _outbox.Entries.Select(envelope => envelope.Type).ShouldContain(WatchdogEventTypes.CheckFailed);
    [Fact] void should_record_that_it_only_observes() => _outbox.Entries.Select(envelope => envelope.PayloadAs<RemedyDenied>()).OfType<RemedyDenied>().Single().Reason.ShouldEqual(nameof(OfflineRemedyReason.ObserveOnly));
}
#endif
