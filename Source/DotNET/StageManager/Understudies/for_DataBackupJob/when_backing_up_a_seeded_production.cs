// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Cryptography;
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.for_DataBackupJob.given;
using Cratis.Showrunner.StageManager.Understudies.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_DataBackupJob;

public class when_backing_up_a_seeded_production : a_seeded_production
{
    async Task Because() => await Run();

    [Fact] void should_succeed() => _result.Succeeded.ShouldBeTrue();
    [Fact] void should_dump_every_claimed_database_that_exists() => _dump.Dumped.ShouldContainOnly(
    [
        "chronicle+main", "Studio+es", "Studio+es+Default", "Studio+es+acme", "Studio", "Studio+acme", "Billing+es", "Billing+es+Default", "Billing",
        "Studio+jobs+acme", "Studio+jobs+Default", "Billing+jobs+Default", "orleans"
    ]);
    [Fact] void should_not_dump_the_database_nobody_claims() => _dump.Dumped.Contains("legacy").ShouldBeFalse();
    [Fact] void should_not_dump_the_databases_of_the_server() => _dump.Dumped.Intersect(["admin", "local", "config"]).ShouldBeEmpty();
    [Fact] void should_lay_a_namespace_out_beside_its_manifest() => Files("Studio", "acme").Select(key => key[Prefix("Studio", "acme").Length..]).ShouldContainOnly(
        ["Events.archive.gz", "ReadModels.archive.gz", "Database-Studio+jobs+acme.archive.gz", "manifest.json"]);
    [Fact] void should_lay_an_event_store_out_with_a_placeholder_namespace() => Files("Studio", null).Select(key => key[Prefix("Studio", null).Length..]).ShouldContainOnly(["EventStore.archive.gz", "manifest.json"]);
    [Fact] void should_lay_the_production_out_with_placeholders() => Files(null, null).Select(key => key[Prefix(null, null).Length..]).ShouldContainOnly(
        ["Cluster.archive.gz", "Database-orleans.archive.gz", "manifest.json"]);
    [Fact] void should_time_the_runs_to_the_second() => Prefix("Studio", "acme").ShouldContain(Takes);
    [Fact] void should_write_one_manifest_per_scope() => _result.Taken.Count.ShouldEqual(6);
    [Fact] void should_list_the_types_in_the_manifest() => ManifestOf("Studio", "acme").Entries.Select(entry => entry.Type).ShouldContainOnly([BackupType.Events, BackupType.ReadModels, BackupType.Database]);
    [Fact] void should_list_the_databases_in_the_manifest() => ManifestOf("Studio", "acme").Entries.Select(entry => entry.Database).ShouldContainOnly(["Studio+es+acme", "Studio+acme", "Studio+jobs+acme"]);
    [Fact] void should_record_the_chronicle_version() => ManifestOf("Studio", "acme").ChronicleVersion.ShouldEqual("19.148.0");
    [Fact] void should_record_the_tiers() => ManifestOf("Studio", "acme").Retention.ShouldEqual(new RetentionPolicy());
    [Fact] void should_record_the_size_of_each_file() => ManifestOf("Studio", "acme").Entries.Single(entry => entry.Type == BackupType.Events).SizeBytes.ShouldEqual(FakeDumpTool.ContentOf("Studio+es+acme").Length);
    [Fact] void should_record_the_checksum_of_each_file() => ManifestOf("Studio", "acme").Entries.Single(entry => entry.Type == BackupType.Events).Sha256
        .ShouldEqual(Convert.ToHexStringLower(SHA256.HashData(FakeDumpTool.ContentOf("Studio+es+acme"))));
    [Fact] void should_store_the_dump_as_it_is() => _storage.ContentOf(Prefix("Studio", "acme") + "Events.archive.gz").ShouldEqual(FakeDumpTool.ContentOf("Studio+es+acme"));
    [Fact] void should_write_the_manifest_after_its_files() => _storage.Writes.IndexOf(Prefix("Studio", "acme") + "manifest.json").ShouldBeGreaterThan(_storage.Writes.IndexOf(Prefix("Studio", "acme") + "ReadModels.archive.gz"));
    [Fact] void should_report_what_nobody_claims_beside_the_backups() => PlanReport.FromJson(new MemoryStream(_storage.ContentOf("ledger/plan.json"))).Unclaimed.ShouldContainOnly(["legacy"]);
    [Fact] void should_leave_no_archive_in_the_work_directory() => Directory.GetFiles(_work).ShouldBeEmpty();
    [Fact] void should_let_the_manifests_alone_list_every_backup() => new BackupCatalog(_storage).ListAsync("ledger", new RetentionPolicy(), _now, CancellationToken.None).GetAwaiter().GetResult().Count.ShouldEqual(6);
}
#endif
