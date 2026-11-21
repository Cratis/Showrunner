// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_enrolling : a_direct_link
{
    async Task Because()
    {
        await Pass();
        await Pass();
    }

    [Fact] void should_claim_on_every_pass() => _direct.Claims.ShouldEqual(2);
    [Fact] void should_hold_the_epoch_of_the_first_claim() => Identity.Epoch.ShouldEqual(1);
    [Fact] void should_keep_the_instance_credential_in_the_secret() => Identity.Credential.ShouldEqual(_direct.Credential);
    [Fact] void should_pin_the_signing_key_of_the_first_claim() => Identity.SigningKey.ShouldEqual(_direct.SigningKey);
    [Fact] void should_never_write_the_credential_token_or_key_to_a_config_map() => _configMaps.Values.Values.Any(value => value.Contains(_direct.Credential) || value.Contains(Token) || value.Contains(_direct.SigningKey)).ShouldBeFalse();
    [Fact] void should_never_keep_the_enrollment_token() => _secrets.Values.Values.Any(value => value.Contains(Token)).ShouldBeFalse();
    [Fact] void should_report_a_connected_link_with_its_epoch() => (Status.Connection, Status.Epoch).ShouldEqual((LinkConnection.Connected, 1L));
    [Fact] void should_deliver_a_heartbeat_each_pass() => Sent<Heartbeat>().Count().ShouldEqual(2);
    [Fact] void should_keep_the_sequence_contiguous() => _direct.Events.Select(envelope => envelope.Sequence).ShouldEqual([1L, 2L]);
}
#endif
