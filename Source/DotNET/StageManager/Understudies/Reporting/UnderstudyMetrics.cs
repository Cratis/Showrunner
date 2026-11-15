// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using Cratis.Showrunner.StageManager.Understudies.Catalog;

namespace Cratis.Showrunner.StageManager.Understudies.Reporting;

/// <summary>
/// The metrics of backups. Labels are the Production, the type of backup and the outcome, and nothing else: a namespace is a tenant and an event store can be named after one,
/// so neither is ever a label, and no person is known here at all.
/// </summary>
public sealed class UnderstudyMetrics : IDisposable
{
    /// <summary>
    /// The name of the meter. Prometheus and OpenTelemetry listeners attach to it by this name.
    /// </summary>
    public const string MeterName = "Cratis.Showrunner.StageManager.Understudy";

    readonly Meter _meter = new(MeterName);
    readonly Counter<long> _backups;
    readonly ConcurrentDictionary<(string Production, BackupType Type), LastBackupStatus> _last = new();
    readonly ConcurrentDictionary<string, (bool Healthy, int Unclaimed)> _health = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="UnderstudyMetrics"/> class.
    /// </summary>
    public UnderstudyMetrics()
    {
        _backups = _meter.CreateCounter<long>("showrunner_understudy_backups_total", description: "Backup runs by outcome, as reported by the operator.");
        _meter.CreateObservableGauge("showrunner_understudy_last_backup_timestamp_seconds", () => _last.Select(entry => Measurement(entry.Key, new DateTimeOffset(entry.Value.TakenAt.UtcDateTime).ToUnixTimeSeconds())), "s", "When the latest backup of a type was taken.");
        _meter.CreateObservableGauge("showrunner_understudy_last_backup_size_bytes", () => _last.Select(entry => Measurement(entry.Key, entry.Value.SizeBytes)), "By", "The size of the files of the latest backup of a type.");
        _meter.CreateObservableGauge("showrunner_understudy_healthy", () => _health.Select(entry => new Measurement<int>(entry.Value.Healthy ? 1 : 0, new KeyValuePair<string, object?>("production", entry.Key))), description: "1 when the latest backup run succeeded.");
        _meter.CreateObservableGauge("showrunner_understudy_unclaimed_databases", () => _health.Select(entry => new Measurement<int>(entry.Value.Unclaimed, new KeyValuePair<string, object?>("production", entry.Key))), description: "Databases no backup rule claims.");
    }

    /// <summary>
    /// Counts one backup run.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <param name="kind">The kind of job: backup, keys or vault-snapshot.</param>
    /// <param name="outcome">The outcome: taken or failed.</param>
    public void CountRun(string production, string kind, string outcome) =>
        _backups.Add(1, new KeyValuePair<string, object?>("production", production), new KeyValuePair<string, object?>("kind", kind), new KeyValuePair<string, object?>("outcome", outcome));

    /// <summary>
    /// Records what the catalog holds.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <param name="lastBackups">The latest backup of each type.</param>
    /// <param name="healthy">Whether the latest run succeeded.</param>
    /// <param name="unclaimed">The number of databases no rule claims.</param>
    public void Observe(string production, IEnumerable<LastBackupStatus> lastBackups, bool healthy, int unclaimed)
    {
        foreach (var type in _last.Keys.Where(key => key.Production == production).ToList())
        {
            _last.TryRemove(type, out _);
        }

        foreach (var last in lastBackups)
        {
            _last[(production, last.Type)] = last;
        }

        _health[production] = (healthy, unclaimed);
    }

    /// <inheritdoc/>
    public void Dispose() => _meter.Dispose();

    static Measurement<long> Measurement((string Production, BackupType Type) key, long value) =>
        new(value, new KeyValuePair<string, object?>("production", key.Production), new KeyValuePair<string, object?>("type", key.Type.ToString()));
}
