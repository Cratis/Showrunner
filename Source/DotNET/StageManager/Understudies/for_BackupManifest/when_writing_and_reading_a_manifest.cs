// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Showrunner.StageManager.Understudies.Catalog;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupManifest;

public class when_writing_and_reading_a_manifest : Specification
{
    BackupManifest _manifest;
    BackupManifest _read;
    string _json;

    void Establish() => _manifest = new(
        BackupManifest.CurrentSchemaVersion,
        "ledger",
        "Studio",
        "acme",
        new(2026, 3, 15, 12, 5, 9, TimeSpan.Zero),
        "19.148.0",
        new RetentionPolicy(),
        [
            new(BackupType.Events, "Studio+es+acme", "Events.archive.gz", 1024, "ab12"),
            new(BackupType.ReadModels, "Studio+acme", "ReadModels.archive.gz", 2048, "cd34")
        ]);

    void Because()
    {
        var bytes = _manifest.ToJson();
        _json = Encoding.UTF8.GetString(bytes);
        _read = BackupManifest.FromJson(new MemoryStream(bytes));
    }

    [Fact] void should_keep_the_production() => _read.Production.ShouldEqual("ledger");
    [Fact] void should_keep_the_scope() => (_read.EventStore, _read.Namespace).ShouldEqual(("Studio", "acme"));
    [Fact] void should_keep_when_it_was_taken() => _read.TakenAt.ShouldEqual(_manifest.TakenAt);
    [Fact] void should_keep_the_chronicle_version() => _read.ChronicleVersion.ShouldEqual("19.148.0");
    [Fact] void should_keep_the_tiers() => _read.Retention.ShouldEqual(new RetentionPolicy());
    [Fact] void should_keep_every_file_with_its_size_and_checksum() => _read.Entries.ShouldEqual(_manifest.Entries);
    [Fact] void should_write_the_type_by_name() => _json.ShouldContain("\"type\": \"ReadModels\"");
    [Fact] void should_write_camel_case_names() => _json.ShouldContain("\"chronicleVersion\"");
}
#endif
