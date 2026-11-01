// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_rotation_is_raised : a_reconciliation
{
    SigningKeyState _stored;
    AnteAttestationKeys _ante;

    protected override Performance CreatePerformance()
    {
        var performance = PerformanceSamples.Performance();
        performance.Spec.SigningKeys.Rotation = 1;
        return performance;
    }

    void Establish()
    {
        _stored = PerformanceSamples.Keys();
        _cluster.Applied.Add(SecretFactory.Create(PerformanceSamples.Performance(), "crm-signing-keys", "signing-keys", new Dictionary<string, string> { [SigningKeyState.SecretKey] = _stored.Serialize() }));
    }

    async Task Because()
    {
        await Reconcile();
        var ante = _cluster.AppliedOf<V1Secret>().Single(secret => secret.Metadata.Name == "crm-ante-secrets");
        _ante = new(ante.Data.Keys.ToList());
    }

    [Fact] void should_use_a_new_invitation_key() => _performance.Status.SigningKeys.InvitationKeyId.ShouldNotEqual(_stored.Invitation.Current.KeyId);
    [Fact] void should_use_a_new_attestation_key() => _performance.Status.SigningKeys.AttestationKeyId.ShouldNotEqual(_stored.Attestation.Current.KeyId);
    [Fact] void should_remember_the_rotation() => _performance.Status.SigningKeys.ObservedRotation.ShouldEqual(1);
    [Fact] void should_pin_the_previous_keys() => _performance.Status.SigningKeys.PinnedPreviousKeys.ShouldEqual(2);
    [Fact] void should_give_ante_the_previous_invitation_public_key() => _ante.Files.Contains("Ante__Invitations__Token__PreviousPublicKeyPems__0").ShouldBeTrue();
    [Fact] void should_give_ante_both_attestation_public_keys() => _ante.Files.Contains("Ante__Invitations__Exchange__Attestation__PublicKeys__1__PublicKeyPem").ShouldBeTrue();

    record AnteAttestationKeys(List<string> Files);
}
#endif
