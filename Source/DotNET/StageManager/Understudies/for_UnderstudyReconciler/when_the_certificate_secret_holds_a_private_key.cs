// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_the_certificate_secret_holds_a_private_key : an_understudy_reconciliation
{
    protected override bool Vault => true;

    protected override UnderstudySpec CreateSpec()
    {
        var spec = UnderstudySamples.Spec();
        spec.Keys = UnderstudySamples.Keys();
        return spec;
    }

    void Establish() => _cluster.AddSecret("customer-certificate", "certificate.pem", "-----BEGIN PRIVATE KEY-----\nabc\n-----END PRIVATE KEY-----");

    async Task Because() => await Reconcile();

    [Fact] void should_not_schedule_the_key_backups() => KeysCronJob.ShouldBeNull();
    [Fact] void should_say_why() => Condition(UnderstudyConditions.KeyBackupsReady).Reason.ShouldEqual("CertificateHasPrivateKey");
    [Fact] void should_not_open_the_keys_bucket() => _storages.Asked.Select(settings => settings.Bucket).ShouldNotContain("ledger-keys");
}
#endif
