// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_keys_are_backed_up : an_understudy_reconciliation
{
    protected override bool Vault => true;

    protected override UnderstudySpec CreateSpec()
    {
        var spec = UnderstudySamples.Spec();
        spec.Keys = UnderstudySamples.Keys();
        return spec;
    }

    V1Container Container => KeysCronJob!.Spec.JobTemplate.Spec.Template.Spec.Containers.Single();

    async Task Because() => await Reconcile();

    [Fact] void should_schedule_the_key_backups() => KeysCronJob.ShouldNotBeNull();
    [Fact] void should_run_them_on_their_own_schedule() => KeysCronJob!.Spec.Schedule.ShouldEqual("0 2 * * *");
    [Fact] void should_start_the_key_job() => string.Join(' ', Container.Args).ShouldEqual("understudy keys");
    [Fact] void should_be_owned_by_the_understudy() => KeysCronJob!.Metadata.OwnerReferences.Single().Uid.ShouldEqual(_understudy.Metadata.Uid);
    [Fact] void should_read_the_certificate_from_its_secret() => Container.Env.Single(variable => variable.Name == UnderstudyJobEnvironment.KeysCertificate).ValueFrom.SecretKeyRef.Name.ShouldEqual("customer-certificate");
    [Fact] void should_read_the_vault_token_from_its_secret() => Container.Env.Single(variable => variable.Name == UnderstudyJobEnvironment.VaultToken).ValueFrom.SecretKeyRef.Name.ShouldEqual("vault-reader");
    [Fact] void should_use_the_credentials_of_the_keys_bucket_only() => Container.Env.Single(variable => variable.Name == "UNDERSTUDY_KEYS_STORAGE_SECRET_KEY").ValueFrom.SecretKeyRef.Name.ShouldEqual("keys-credentials");
    [Fact] void should_not_hand_the_data_credentials_to_the_key_job() => Container.Env.Any(variable => variable.ValueFrom?.SecretKeyRef?.Name == "backup-credentials").ShouldBeFalse();
    [Fact] void should_not_hand_the_database_to_the_key_job() => Container.Env.Any(variable => variable.Name == UnderstudyJobEnvironment.MongoDBConnection).ShouldBeFalse();
    [Fact] void should_tell_the_job_the_vault_address() => UnderstudyJobConfiguration.FromJson(Container.Env.Single(variable => variable.Name == UnderstudyJobEnvironment.Configuration).Value).VaultAddress.ShouldEqual("https://vault.example:8200");
    [Fact] void should_tell_the_job_the_keys_bucket() => UnderstudyJobConfiguration.FromJson(Container.Env.Single(variable => variable.Name == UnderstudyJobEnvironment.Configuration).Value).KeysStorage!.Bucket.ShouldEqual("ledger-keys");
    [Fact] void should_check_the_rules_of_the_keys_bucket() => Keys.ExpiryDays.ShouldEqual(35);
    [Fact] void should_say_the_key_backups_are_ready() => Condition(UnderstudyConditions.KeyBackupsReady).Status.ShouldEqual("True");
    [Fact] void should_not_put_the_certificate_in_the_job() => Container.Env.Any(variable => variable.Value?.Contains("BEGIN CERTIFICATE", StringComparison.Ordinal) == true).ShouldBeFalse();
    [Fact] void should_still_schedule_the_database_backups() => BackupCronJob.ShouldNotBeNull();
    [Fact] void should_keep_the_two_buckets_apart() => _storages.Asked.Select(settings => settings.Bucket).Distinct().ShouldContainOnly(["ledger-backups", "ledger-keys"]);
}
#endif
