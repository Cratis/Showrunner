// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_direct_has_been_unreachable_too_long_for_a_remedy : a_direct_link
{
    OutboxSnapshot _outbox;

    async Task Because()
    {
        PublishReadiness(RemedyPolicyMode.Review, RemedyPolicyMode.Automatic, true);
        _endpointStatus["/ready"] = HttpStatusCode.ServiceUnavailable;
        await Pass();
        _direct.Unreachable = true;
        _time.Advance(TimeSpan.FromMinutes(10));
        await Pass();
        await Pass();
        _outbox = await _store.LoadOutbox(CancellationToken.None);
    }

    [Fact] void should_not_run_a_remedy_it_cannot_be_sure_it_is_entitled_to() => _deployments.Restarted.ShouldBeEmpty();
    [Fact] void should_record_a_denial_naming_the_uncertain_fencing() => _outbox.Entries.Select(envelope => envelope.PayloadAs<RemedyDenied>()).OfType<RemedyDenied>().Single().Reason.ShouldEqual(nameof(OfflineRemedyReason.FencingUncertain));
    [Fact] void should_record_the_denial_once_not_on_every_pass() => _outbox.Entries.Count(envelope => envelope.Type == WatchdogEventTypes.RemedyDenied).ShouldEqual(1);
}
#endif
