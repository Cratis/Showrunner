// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_direct_is_unreachable_and_the_policy_permits_a_remedy : a_direct_link
{
    OutboxSnapshot _outbox;

    async Task Because()
    {
        PublishReadiness(RemedyPolicyMode.Review, RemedyPolicyMode.Automatic, true);
        _endpointStatus["/ready"] = HttpStatusCode.ServiceUnavailable;
        await Pass();
        _direct.Unreachable = true;
        _time.Advance(TimeSpan.FromMinutes(1));
        await Pass();
        _outbox = await _store.LoadOutbox(CancellationToken.None);
    }

    [Fact] void should_run_the_exact_remedy_once() => _deployments.Restarted.ShouldEqual(["shop/web"]);
    [Fact] void should_record_the_attempt_as_offline_without_an_authorization() => _outbox.Entries.Select(envelope => envelope.PayloadAs<RemedyAttempted>()).OfType<RemedyAttempted>().Single().ShouldEqual(new RemedyAttempted(Restart, Resource, 1, true, Guid.Empty, 1, 1));
    [Fact] void should_queue_what_happened_for_direct() => _outbox.Entries.Select(envelope => envelope.Type).ShouldContain(WatchdogEventTypes.RemedySucceeded);
}
#endif
