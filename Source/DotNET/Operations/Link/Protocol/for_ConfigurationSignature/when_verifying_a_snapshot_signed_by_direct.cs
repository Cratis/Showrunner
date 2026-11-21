// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using System.Text.Json;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Link.Protocol.for_ConfigurationSignature;

public class when_verifying_a_snapshot_signed_by_direct : Specification
{
    SignedWatchdogConfiguration _signed;
    bool _verified;
    string _canonical;
    string _digest;
    string _resigned;
    bool _verifiedWithAnotherKey;

    void Because()
    {
        _signed = JsonSerializer.Deserialize<SignedWatchdogConfiguration>(DirectFixtures.Signed, WatchdogJson.Options)!;
        _verified = ConfigurationSignature.Verify(_signed, DirectFixtures.Key);
        _canonical = Encoding.UTF8.GetString(ConfigurationSignature.Canonical(_signed.Snapshot));
        _digest = ConfigurationDigest.Of(_signed.Snapshot);
        _resigned = ConfigurationSignature.Sign(_signed.Snapshot, DirectFixtures.Key).Signature;
        _verifiedWithAnotherKey = ConfigurationSignature.Verify(_signed, ConfigurationSignature.NewKey());
    }

    [Fact] void should_verify_under_the_key_direct_signed_with() => _verified.ShouldBeTrue();
    [Fact] void should_not_verify_under_another_key() => _verifiedWithAnotherKey.ShouldBeFalse();
    [Fact] void should_build_the_canonical_form_direct_signs() => _canonical.ShouldEqual(DirectFixtures.Canonical);
    [Fact] void should_compute_the_digest_direct_publishes() => _digest.ShouldEqual(DirectFixtures.Digest);
    [Fact] void should_produce_the_same_signature_direct_does() => _resigned.ShouldEqual(DirectFixtures.Signature);
}
#endif
