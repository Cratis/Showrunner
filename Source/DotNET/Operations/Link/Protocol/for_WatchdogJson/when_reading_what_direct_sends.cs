// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Link.Protocol.for_WatchdogJson;

public class when_reading_what_direct_sends : Specification
{
    SignedWatchdogConfiguration _signed;
    WatchdogClaimResponse _claim;
    WatchdogEventEnvelope _envelope;
    WatchdogEventBatchReceipt _receipt;
    WatchdogRemedyAuthorizations _authorizations;

    void Because()
    {
        _signed = JsonSerializer.Deserialize<SignedWatchdogConfiguration>(DirectFixtures.Signed, WatchdogJson.Options)!;
        _claim = JsonSerializer.Deserialize<WatchdogClaimResponse>(DirectFixtures.ClaimResponse, WatchdogJson.Options)!;
        _envelope = JsonSerializer.Deserialize<WatchdogEventEnvelope>(DirectFixtures.Envelope, WatchdogJson.Options)!;
        _receipt = JsonSerializer.Deserialize<WatchdogEventBatchReceipt>(DirectFixtures.Receipt, WatchdogJson.Options)!;
        _authorizations = JsonSerializer.Deserialize<WatchdogRemedyAuthorizations>(DirectFixtures.Authorizations, WatchdogJson.Options)!;
    }

    [Fact] void should_read_the_snapshot_with_enums_as_strings() => _signed.Snapshot.Checks[0].Method.ShouldEqual(WatchdogHttpMethod.Head);
    [Fact] void should_read_remedy_modes_as_strings() => _signed.Snapshot.Remedies[0].OnlineMode.ShouldEqual(RemedyPolicyMode.Automatic);
    [Fact] void should_read_durations_in_the_constant_format() => _signed.Snapshot.Remedies[0].Cooldown.ShouldEqual(TimeSpan.FromMinutes(10));
    [Fact] void should_read_the_secrets() => _signed.Snapshot.Secrets["probe"].ShouldEqual("token");
    [Fact] void should_read_the_first_claim_with_its_credential_and_key() => (_claim.Credential, _claim.SigningKey, _claim.FencingEpoch).ShouldEqual(("credential", DirectFixtures.Key, 1L));
    [Fact] void should_read_the_envelope_payload_as_its_type() => _envelope.PayloadAs<CheckFailed>()!.ConsecutiveFailures.ShouldEqual(2);
    [Fact] void should_read_the_receipt_gaps() => _receipt.Gaps.Single().ShouldEqual(new WatchdogSequenceGap(3, 4));
    [Fact] void should_read_the_authorization_epoch_and_revision() => (_authorizations.Authorized[0].FencingEpoch, _authorizations.Authorized[0].ConfigurationRevision).ShouldEqual((1L, 3L));
}
#endif
