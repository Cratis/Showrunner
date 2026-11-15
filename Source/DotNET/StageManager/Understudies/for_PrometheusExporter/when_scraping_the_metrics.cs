// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.Reporting;

namespace Cratis.Showrunner.StageManager.Understudies.for_PrometheusExporter;

public class when_scraping_the_metrics : Specification
{
    const string Production = "metrics-spec-production";

    UnderstudyMetrics _metrics;
    PrometheusExporter _exporter;
    string[] _lines;

    void Establish()
    {
        _metrics = new();
        _exporter = new();
    }

    void Because()
    {
        _metrics.CountRun(Production, "backup", "taken");
        _metrics.CountRun(Production, "backup", "taken");
        _metrics.CountRun(Production, "backup", "failed");
        _metrics.Observe(
            Production,
            [new() { Type = BackupType.Events, TakenAt = new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero), SizeBytes = 2048, Files = 3 }],
            true,
            2);
        _lines = _exporter.Render().Split('\n').Where(line => line.Contains(Production, StringComparison.Ordinal)).ToArray();
    }

    void Destroy()
    {
        _exporter.Dispose();
        _metrics.Dispose();
    }

    string Line(string metric) => _lines.Single(line => line.StartsWith(metric + "{", StringComparison.Ordinal) && !line.StartsWith('#'));

    [Fact] void should_count_the_taken_backups() => _lines.Any(line => line.StartsWith("showrunner_understudy_backups_total", StringComparison.Ordinal) && line.Contains("outcome=\"taken\"", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_add_the_runs_up() => _lines.Single(line => line.Contains("outcome=\"taken\"", StringComparison.Ordinal)).EndsWith(" 2", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_count_the_failed_backups_apart() => _lines.Count(line => line.StartsWith("showrunner_understudy_backups_total", StringComparison.Ordinal)).ShouldEqual(2);
    [Fact] void should_report_when_the_last_backup_of_a_type_was_taken() => Line("showrunner_understudy_last_backup_timestamp_seconds").EndsWith($" {new DateTimeOffset(2026, 3, 15, 12, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds()}", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_report_the_size_of_the_last_backup_of_a_type() => Line("showrunner_understudy_last_backup_size_bytes").EndsWith(" 2048", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_label_the_type() => Line("showrunner_understudy_last_backup_size_bytes").ShouldContain("type=\"Events\"");
    [Fact] void should_report_health() => Line("showrunner_understudy_healthy").EndsWith(" 1", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_report_the_unclaimed_databases() => Line("showrunner_understudy_unclaimed_databases").EndsWith(" 2", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_label_nothing_but_production_type_kind_and_outcome() => _lines.All(line => !line.Contains("namespace=", StringComparison.Ordinal) && !line.Contains("tenant", StringComparison.Ordinal) && !line.Contains("eventstore", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    [Fact] void should_name_the_metrics_for_prometheus() => _exporter.Render().ShouldContain("# TYPE showrunner_understudy_backups_total counter");
    [Fact] void should_describe_the_gauges_as_gauges() => _exporter.Render().ShouldContain("# TYPE showrunner_understudy_healthy gauge");
}
#endif
