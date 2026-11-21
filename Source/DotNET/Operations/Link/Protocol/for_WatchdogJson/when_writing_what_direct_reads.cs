// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Link.Protocol.for_WatchdogJson;

public class when_writing_what_direct_reads : Specification
{
    static readonly Guid _resource = Guid.Parse("7d3f7f1e-9a53-4e0b-9f0e-6c1b1f2a3b4c");
    static readonly DateTimeOffset _at = new(2026, 9, 29, 12, 0, 0, TimeSpan.Zero);

    string _claim;
    string _envelope;
    string _heartbeat;
    string _remedyRequest;

    void Because()
    {
        _claim = JsonSerializer.Serialize(new WatchdogClaimRequest("1.2.3", "session-1", WatchdogVariant.Slim), WatchdogJson.Options);
        _envelope = JsonSerializer.Serialize(
            WatchdogEventEnvelope.For("prod-edge-a", 7, Guid.Parse("11111111-1111-1111-1111-111111111111"), Guid.Empty, _at, new CheckFailed("readiness", _resource, 503, "unexpected status", 2, 3)),
            WatchdogJson.Options);
        _heartbeat = JsonSerializer.Serialize(
            WatchdogEventEnvelope.For("prod-edge-a", 8, Guid.Parse("33333333-3333-3333-3333-333333333333"), Guid.Empty, _at, new Heartbeat(3, "1.2.3", true, 1)),
            WatchdogJson.Options);
        _remedyRequest = JsonSerializer.Serialize(
            new WatchdogRemedyRequest(Guid.Parse("22222222-2222-2222-2222-222222222222"), "restart-web", 3, Guid.Parse("11111111-1111-1111-1111-111111111111"), _at),
            WatchdogJson.Options);
    }

    [Fact] void should_write_the_claim_as_direct_does() => _claim.ShouldEqual(DirectFixtures.ClaimRequest);
    [Fact] void should_write_an_envelope_as_direct_does() => _envelope.ShouldEqual(DirectFixtures.Envelope);
    [Fact] void should_write_a_heartbeat_as_direct_does() => _heartbeat.ShouldEqual(DirectFixtures.Heartbeat);
    [Fact] void should_write_a_remedy_request_as_direct_does() => _remedyRequest.ShouldEqual(DirectFixtures.RemedyRequest);
}
#endif
