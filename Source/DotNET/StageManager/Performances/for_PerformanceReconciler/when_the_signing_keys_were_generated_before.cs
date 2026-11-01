// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_signing_keys_were_generated_before : a_reconciliation
{
    SigningKeyState _stored;

    void Establish()
    {
        _stored = PerformanceSamples.Keys();
        var secret = SecretFactory.Create(PerformanceSamples.Performance(), "crm-signing-keys", "signing-keys", new Dictionary<string, string> { [SigningKeyState.SecretKey] = _stored.Serialize() });
        _cluster.Applied.Add(secret);
    }

    Task Because() => Reconcile();

    [Fact] void should_keep_the_invitation_key() => _performance.Status.SigningKeys.InvitationKeyId.ShouldEqual(_stored.Invitation.Current.KeyId);
    [Fact] void should_keep_the_attestation_key() => _performance.Status.SigningKeys.AttestationKeyId.ShouldEqual(_stored.Attestation.Current.KeyId);
}
#endif
