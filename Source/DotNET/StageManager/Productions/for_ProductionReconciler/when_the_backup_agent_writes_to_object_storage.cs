// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json.Nodes;
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_backup_agent_writes_to_object_storage : a_production_reconciliation
{
    PerconaServerMongoDB _database;

    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.Managed();
        spec.Database.Backup.Enabled = true;
        spec.Database.Backup.Storage = new()
        {
            Bucket = "ledger-backups",
            Endpoint = "https://objects.example.com/",
            Region = "europe-1",
            CredentialsSecretName = "ledger-backup-credentials"
        };
        return spec;
    }

    void Establish()
    {
        _cluster.SecretValues["apps/ledger-backup-credentials/AWS_ACCESS_KEY_ID"] = "key";
        _cluster.SecretValues["apps/ledger-backup-credentials/AWS_SECRET_ACCESS_KEY"] = "secret";
    }

    async Task Because()
    {
        await Reconcile();
        _database = _cluster.AppliedNamed<PerconaServerMongoDB>("ledger-mongodb")!;
    }

    JsonNode Minio => _database.Spec["backup"]!["storages"]![ProductionDatabaseBackupStorage.StorageName]!;

    [Fact] void should_use_the_minio_storage_type() => Minio["type"]!.GetValue<string>().ShouldEqual("minio");
    [Fact] void should_name_the_bucket() => Minio["minio"]!["bucket"]!.GetValue<string>().ShouldEqual("ledger-backups");
    [Fact] void should_give_the_endpoint_without_a_scheme() => Minio["minio"]!["endpointUrl"]!.GetValue<string>().ShouldEqual("objects.example.com");
    [Fact] void should_connect_securely() => Minio["minio"]!["secure"]!.GetValue<bool>().ShouldBeTrue();
    [Fact] void should_address_the_bucket_by_path() => Minio["minio"]!["forcePathStyle"]!.GetValue<bool>().ShouldBeTrue();
    [Fact] void should_keep_the_backups_under_the_default_prefix() => Minio["minio"]!["prefix"]!.GetValue<string>().ShouldEqual("pbm");
    [Fact] void should_name_the_region() => Minio["minio"]!["region"]!.GetValue<string>().ShouldEqual("europe-1");
    [Fact] void should_name_the_credentials_secret() => Minio["minio"]!["credentialsSecret"]!.GetValue<string>().ShouldEqual("ledger-backup-credentials");
    [Fact] void should_schedule_the_default_tasks() => _database.Spec["backup"]!["tasks"]!.AsArray().Select(task => task!["name"]!.GetValue<string>()).ShouldContainOnly("hourly", "daily", "weekly", "monthly");
    [Fact] void should_write_every_task_to_the_storage() => _database.Spec["backup"]!["tasks"]!.AsArray().All(task => task!["storageName"]!.GetValue<string>() == ProductionDatabaseBackupStorage.StorageName).ShouldBeTrue();
    [Fact] void should_not_use_the_s3_storage_type() => _database.Spec["backup"]!["storages"]![ProductionDatabaseBackupStorage.StorageName]!["s3"].ShouldBeNull();
}
#endif
