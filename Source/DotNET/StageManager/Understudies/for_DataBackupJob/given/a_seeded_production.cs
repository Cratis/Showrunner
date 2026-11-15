// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.Discovery;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;

namespace Cratis.Showrunner.StageManager.Understudies.for_DataBackupJob.given;

/// <summary>
/// A seeded Production: Studio with its default namespace and acme, Billing with its default namespace, their databases, a declared per-namespace database,
/// the database of Orleans, and one database nobody claims.
/// </summary>
public abstract class a_seeded_production : Specification
{
    protected static readonly DateTimeOffset _now = new(2026, 3, 15, 12, 30, 45, 123, TimeSpan.Zero);

    protected const string Takes = "20260315T123045Z";

    protected InMemoryObjectStorage _storage;
    protected FakeDumpTool _dump;
    protected FakeChronicleDirectory _directory;
    protected FakeDatabaseCatalog _databases;
    protected UnderstudyJobConfiguration _configuration;
    protected BackupRunResult _result;
    protected string _work;

    protected string Prefix(string? eventStore, string? @namespace) => BackupLayout.RunPrefix("ledger", eventStore, @namespace, _now);

    protected IEnumerable<string> Files(string? eventStore, string? @namespace) =>
        _storage.Keys.Where(key => key.StartsWith(Prefix(eventStore, @namespace), StringComparison.Ordinal));

    protected BackupManifest ManifestOf(string? eventStore, string? @namespace) =>
        BackupManifest.FromJson(new MemoryStream(_storage.ContentOf(BackupLayout.ManifestKey(Prefix(eventStore, @namespace)))));

    void Establish()
    {
        _work = Path.Combine(Path.GetTempPath(), $"understudy-work-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_work);
        _storage = new(new FixedTimeProvider(_now));
        _dump = new();
        _directory = new(new([new("Studio", ["Default", "acme"]), new("Billing", ["Default"])]));
        _databases = new(
        [
            "chronicle+main",
            "Studio+es",
            "Studio+es+Default",
            "Studio+es+acme",
            "Studio",
            "Studio+acme",
            "Billing+es",
            "Billing+es+Default",
            "Billing",
            "Studio+jobs+acme",
            "Studio+jobs+Default",
            "Billing+jobs+Default",
            "orleans",
            "legacy",
            "admin",
            "local",
            "config"
        ]);
        _configuration = new(
            UnderstudyJobKinds.Backup,
            "ledger",
            "19.148.0",
            string.Empty,
            ["{eventStore}+jobs+{namespace}", "orleans"],
            false,
            new RetentionPolicy(),
            new(),
            null,
            35,
            string.Empty);
    }

    void Destroy() => Directory.Delete(_work, true);

    protected async Task Run() =>
        _result = await new DataBackupJob(_storage, _directory, _databases, _dump, new FixedTimeProvider(_now), _work, Substitute.For<ILogger<DataBackupJob>>())
            .RunAsync(_configuration, CancellationToken.None);
}
#endif
