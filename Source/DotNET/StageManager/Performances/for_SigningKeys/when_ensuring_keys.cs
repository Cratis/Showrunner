// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_SigningKeys;

public class when_ensuring_keys : Specification
{
    SigningKeyState _first;
    SigningKeyState _again;
    SigningKeyState _rotated;
    SigningKeyState _rotatedTwice;
    SigningKeyState _afterTrim;

    void Because()
    {
        _first = SigningKeys.Ensure(null, 0, 1);
        _again = SigningKeys.Ensure(_first, 0, 1);
        _rotated = SigningKeys.Ensure(_first, 1, 1);
        _rotatedTwice = SigningKeys.Ensure(_rotated, 2, 1);
        _afterTrim = SigningKeys.Ensure(_rotatedTwice, 2, 0);
    }

    [Fact] void should_generate_a_private_key_the_framework_can_read() => _first.Invitation.Current.PrivateKeyPem.StartsWith("-----BEGIN PRIVATE KEY-----", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_use_separate_keys_for_invitations_and_attestations() => _first.Invitation.Current.KeyId.ShouldNotEqual(_first.Attestation.Current.KeyId);
    [Fact] void should_keep_the_keys_when_nothing_was_raised() => _again.Invitation.Current.KeyId.ShouldEqual(_first.Invitation.Current.KeyId);
    [Fact] void should_replace_the_key_when_the_rotation_is_raised() => _rotated.Invitation.Current.KeyId.ShouldNotEqual(_first.Invitation.Current.KeyId);
    [Fact] void should_pin_the_public_key_it_replaced() => _rotated.Invitation.Previous.Single().KeyId.ShouldEqual(_first.Invitation.Current.KeyId);
    [Fact] void should_pin_only_the_public_half() => _rotated.Invitation.Previous.Single().PublicKeyPem.Contains("PRIVATE", StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_remember_the_rotation() => _rotated.Rotation.ShouldEqual(1);
    [Fact] void should_keep_no_more_previous_keys_than_it_is_told_to() => _rotatedTwice.Attestation.Previous.Count.ShouldEqual(1);
    [Fact] void should_keep_the_newest_previous_key() => _rotatedTwice.Attestation.Previous.Single().KeyId.ShouldEqual(_rotated.Attestation.Current.KeyId);
    [Fact] void should_stop_pinning_when_told_to_retain_none() => _afterTrim.Invitation.Previous.Count.ShouldEqual(0);
    [Fact] void should_survive_being_stored_in_a_secret() => SigningKeyState.Deserialize(_rotated.Serialize())!.Invitation.Current.KeyId.ShouldEqual(_rotated.Invitation.Current.KeyId);
}
#endif
