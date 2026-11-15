// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_reconciling_a_valid_understudy : an_understudy_reconciliation
{
    V1Container Container => BackupCronJob!.Spec.JobTemplate.Spec.Template.Spec.Containers.Single();

    V1EnvVar? Environment(string name) => Container.Env.SingleOrDefault(variable => variable.Name == name);

    async Task Because() => await Reconcile();

    [Fact] void should_be_scheduled() => _phase.ShouldEqual(UnderstudyPhase.Scheduled);
    [Fact] void should_schedule_a_cron_job() => BackupCronJob.ShouldNotBeNull();
    [Fact] void should_run_it_on_the_schedule() => BackupCronJob!.Spec.Schedule.ShouldEqual("15 * * * *");
    [Fact] void should_not_run_two_at_once() => BackupCronJob!.Spec.ConcurrencyPolicy.ShouldEqual("Forbid");
    [Fact] void should_not_be_suspended() => BackupCronJob!.Spec.Suspend.ShouldEqual(false);
    [Fact] void should_not_retry_a_failed_backup() => BackupCronJob!.Spec.JobTemplate.Spec.BackoffLimit.ShouldEqual(0);
    [Fact] void should_not_restart_the_pod() => BackupCronJob!.Spec.JobTemplate.Spec.Template.Spec.RestartPolicy.ShouldEqual("Never");
    [Fact] void should_be_owned_by_the_understudy() => BackupCronJob!.Metadata.OwnerReferences.Single().Uid.ShouldEqual(_understudy.Metadata.Uid);
    [Fact] void should_remove_with_the_understudy() => BackupCronJob!.Metadata.OwnerReferences.Single().BlockOwnerDeletion.ShouldEqual(true);
    [Fact] void should_belong_to_the_understudy() => BackupCronJob!.Metadata.Labels[UnderstudyMetadata.UnderstudyLabel].ShouldEqual("ledger-backups");
    [Fact] void should_label_its_jobs() => BackupCronJob!.Spec.JobTemplate.Metadata.Labels[UnderstudyMetadata.UnderstudyLabel].ShouldEqual("ledger-backups");
    [Fact] void should_run_the_understudy_image_of_the_release() => Container.Image.ShouldEqual("ghcr.io/cratis/showrunner-understudy:1.2.3");
    [Fact] void should_start_the_backup_job() => string.Join(' ', Container.Args).ShouldEqual("understudy backup");
    [Fact] void should_read_the_database_connection_from_its_secret() => Environment(UnderstudyJobEnvironment.MongoDBConnection)!.ValueFrom.SecretKeyRef.Name.ShouldEqual("ledger-mongo");
    [Fact] void should_read_the_chronicle_connection_from_the_connection_secret() => Environment(UnderstudyJobEnvironment.ChronicleConnection)!.ValueFrom.SecretKeyRef.Name.ShouldEqual("ledger-chronicle-connection");
    [Fact] void should_read_the_storage_credentials_from_their_secret() => Environment("UNDERSTUDY_STORAGE_SECRET_KEY")!.ValueFrom.SecretKeyRef.Name.ShouldEqual("backup-credentials");
    [Fact] void should_not_copy_a_credential_into_the_job() => Container.Env.Any(variable => variable.Value?.Contains("secret-key-value", StringComparison.Ordinal) == true || variable.Value?.Contains("access-key-value", StringComparison.Ordinal) == true).ShouldBeFalse();
    [Fact] void should_tell_the_job_what_to_back_up() => UnderstudyJobConfiguration.FromJson(Environment(UnderstudyJobEnvironment.Configuration)!.Value).Databases.ShouldContainOnly(["{eventStore}+jobs+{namespace}", "orleans"]);
    [Fact] void should_tell_the_job_the_chronicle_version() => UnderstudyJobConfiguration.FromJson(Environment(UnderstudyJobEnvironment.Configuration)!.Value).ChronicleVersion.ShouldEqual("19.148.0");
    [Fact] void should_tell_the_job_the_retention() => UnderstudyJobConfiguration.FromJson(Environment(UnderstudyJobEnvironment.Configuration)!.Value).Retention.MaxAgeDays.ShouldEqual(35);
    [Fact] void should_run_as_a_non_root_user() => BackupCronJob!.Spec.JobTemplate.Spec.Template.Spec.SecurityContext.RunAsNonRoot.ShouldEqual(true);
    [Fact] void should_not_write_to_the_file_system_of_the_image() => Container.SecurityContext.ReadOnlyRootFilesystem.ShouldEqual(true);
    [Fact] void should_give_it_scratch_space_to_dump_into() => BackupCronJob!.Spec.JobTemplate.Spec.Template.Spec.Volumes.Single(volume => volume.Name == "work").EmptyDir.SizeLimit.ToString().ShouldEqual("10Gi");
    [Fact] void should_not_schedule_key_backups() => KeysCronJob.ShouldBeNull();
    [Fact] void should_say_it_is_scheduled() => Condition(UnderstudyConditions.Scheduled).Status.ShouldEqual("True");
    [Fact] void should_say_the_production_was_found() => Condition(UnderstudyConditions.ProductionFound).Status.ShouldEqual("True");
    [Fact] void should_say_the_bucket_is_reachable() => Condition(UnderstudyConditions.StorageReady).Status.ShouldEqual("True");
    [Fact] void should_say_no_backup_has_been_taken_yet() => Condition(UnderstudyConditions.BackupsHealthy).Reason.ShouldEqual("NoBackupYet");
    [Fact] void should_not_put_a_secret_value_in_the_status() => StatusAsJson.Contains("secret-key-value", StringComparison.Ordinal).ShouldBeFalse();
}
#endif
