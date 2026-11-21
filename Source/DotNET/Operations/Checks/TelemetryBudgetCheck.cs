// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Watches the telemetry pipeline's own budgets in Prometheus: refused, dropped or failed telemetry in the last 15 minutes, delta
/// accumulation drops, discarded Loki samples and Tempo spans, oversized scrapes (named from the active targets), the head series count,
/// container memory against its limit, and whether the telemetry Services are scraped at all. Not being able to read a budget is a
/// finding, never a healthy skip; one failing query never hides the findings of the others.
/// </summary>
/// <param name="prometheus">The Prometheus reader.</param>
/// <param name="options">The options.</param>
public sealed class TelemetryBudgetCheck(IPrometheusReader prometheus, TelemetryBudgetOptions options) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "telemetry-budget";

    const string Collector = "collector";
    const string Series = "series";
    const string MemoryLimits = "memory-limits";
    const string Memory = "memory";
    const string Scraping = "scraping";
    const string ScrapeKeyPrefix = "telemetry:scrape:";
    const string ScrapeTargetKeyPrefix = "telemetry:scrape-target:";

    static readonly TimeSpan _budget = TimeSpan.FromSeconds(12);
    static readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(2);
    static readonly string[] _limitTerms = ["sample limit", "label limit", "label_limit", "label name", "label value", "body size", "body_size_limit"];
    static readonly string[] _requiredMetrics = [Series, Memory, MemoryLimits];
    static readonly JsonSerializerOptions _compact = new() { WriteIndented = false };

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        var deadline = Earliest(context.TimeProvider.GetUtcNow() + _budget, context.CollectionDeadline);
        var findings = new Dictionary<string, string>(StringComparer.Ordinal);
        var unreadable = new List<string>();
        var limits = new Dictionary<string, double>(StringComparer.Ordinal);

        foreach (var (check, expression, message) in Queries())
        {
            try
            {
                var samples = await Query(expression, deadline, context, cancellationToken);
                if (_requiredMetrics.Contains(check) && samples.Count == 0)
                {
                    throw new InvalidOperationException("required health metrics are missing");
                }

                switch (check)
                {
                    case MemoryLimits:
                        foreach (var sample in samples)
                        {
                            limits[Identity(sample)] = sample.Value;
                        }

                        break;
                    case Scraping:
                        var healthy = samples.Where(sample => sample.Value > 0).Select(sample => Required(sample, "service")).ToHashSet(StringComparer.Ordinal);
                        foreach (var service in options.ScrapedServices.Where(service => !healthy.Contains(service)))
                        {
                            findings[$"telemetry:scraping:{service}"] = $"{service}: {message}";
                        }

                        break;
                    default:
                        Evaluate(check, message, samples, limits, findings);
                        break;
                }
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                unreadable.Add($"{check}: {Redaction.Summarize(error.Message)}");
            }
        }

        try
        {
            AttributeScrapeFailures(findings, await Targets(deadline, context, cancellationToken));
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            unreadable.Add($"scrape-targets: {Redaction.Summarize(error.Message)}");
        }

        if (unreadable.Count > 0)
        {
            findings["telemetry:api"] = $"could not read Prometheus telemetry budgets: {string.Join("; ", unreadable)}; inspect Prometheus availability and watchdog query access";
        }

        return [.. findings.Select(entry => new Finding(entry.Key, entry.Value))];
    }

    static DateTimeOffset Earliest(DateTimeOffset first, DateTimeOffset second) => first < second ? first : second;

    static string Required(PrometheusSample sample, string label) =>
        sample.Label(label) ?? throw new InvalidOperationException("query returned an unreadable sample");

    static string Identity(PrometheusSample sample) => $"{Required(sample, "namespace")}/{Required(sample, "pod")}/{Required(sample, "container")}";

    static string Labels(PrometheusSample sample)
    {
        var sorted = new JsonObject();
        foreach (var label in sample.Labels.OrderBy(label => label.Key, StringComparer.Ordinal))
        {
            sorted[label.Key] = label.Value;
        }

        return sorted.ToJsonString(_compact);
    }

    static string General(double value) => value.ToString("G6", CultureInfo.InvariantCulture).Replace('E', 'e');

    static string MiB(double bytes) => (bytes / 1048576).ToString("F1", CultureInfo.InvariantCulture);

    static void AttributeScrapeFailures(Dictionary<string, string> findings, IReadOnlyList<ScrapeTarget> targets)
    {
        var failing = targets
            .Where(target => target.Health == "down" && _limitTerms.Any(term => target.LastError.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        foreach (var target in failing)
        {
            findings[$"{ScrapeTargetKeyPrefix}{target.Job}/{target.Instance}"] =
                $"Prometheus rejected the whole scrape of {target.Job}/{target.Instance}: {Redaction.Summarize(target.LastError)}. "
                + "If this is the collector's :8889 target, all app metrics were lost for that scrape. "
                + "Inspect target cardinality/labels and scrape errors before raising limits";
        }

        // These counters are global, not per target. Name current offenders from the target API; a recovered historical
        // offender cannot be attributed honestly.
        var names = string.Join(", ", failing.Select(target => $"{target.Job}/{target.Instance}"));
        foreach (var key in findings.Keys.Where(key => key.StartsWith(ScrapeKeyPrefix, StringComparison.Ordinal)).ToList())
        {
            findings[key] += names.Length > 0
                ? $"; current limit-failing targets: {names}"
                : "; offending target already recovered or could not be attributed; inspect Prometheus scrape logs for the 15m window";
        }
    }

    IEnumerable<(string Check, string Expression, string Message)> Queries()
    {
        var ns = options.PlatformNamespace;
        var services = string.Join('|', options.ScrapedServices);
        yield return (Collector, "sum by (signal, receiver, exporter, processor) (increase(label_replace({__name__=~\"otelcol_(receiver_refused|exporter_send_failed|exporter_enqueue_failed|processor_refused|processor_dropped)_.*\",processor!~\"filter/(orleans-runtime|orleans-application|telemetry-noise|otlp-logs-opt-in)\"}, \"signal\", \"$1\", \"__name__\", \"(.+)\")[15m:1m])) > 0",
            "Collector refused, dropped or failed to deliver telemetry in the last 15m; inspect collector memory-limiter and exporter queue metrics and backend health");
        yield return ("delta", "increase(otelcol_deltatocumulative_datapoints_linear_total{error!=\"\"}[15m]) > 0",
            "Collector delta accumulation dropped datapoints in the last 15m; inspect the error label, stream limit and count emission ordering");
        yield return ("loki", "sum by (reason) (increase(loki_discarded_samples_total[15m])) > 0",
            "Loki discarded log samples in the last 15m; inspect the reason and Loki ingestion/stream limits");
        yield return ("tempo", "sum by (reason) (increase(tempo_discarded_spans_total[15m])) > 0",
            "Tempo discarded spans in the last 15m; inspect the reason and Tempo ingestion/trace-size limits; do not raise sampling");
        yield return ("scrape", "sum by (limit_counter, job, instance) (increase(label_replace({__name__=~\"prometheus_target_scrapes_exceeded_(sample|body_size)_limit_total|prometheus_target_scrape_pool_exceeded_label_limits_total\"}, \"limit_counter\", \"$1\", \"__name__\", \"(.+)\")[15m:1m])) > 0",
            "Prometheus rejected oversized scrapes in the last 15m; inspect failing targets' scrape errors and remove excess series/labels rather than raising limits");
        yield return (Series, "prometheus_tsdb_head_series",
            "Prometheus head series exceed the budget; inspect high-cardinality targets and labels before adding instrumentation");
        yield return (MemoryLimits, $"max by (namespace, pod, container) (kube_pod_container_resource_limits{{namespace=\"{ns}\",resource=\"memory\"}}) > 0",
            "Container memory limits are unreadable");
        yield return (Memory, $"max by (namespace, pod, container) (container_memory_working_set_bytes{{namespace=\"{ns}\",container!=\"\",container!=\"POD\"}}) / on (namespace, pod, container) (max by (namespace, pod, container) (kube_pod_container_resource_limits{{namespace=\"{ns}\",resource=\"memory\"}}) > 0)",
            "Container working set exceeds 90% of its memory limit; inspect ingest rate, queues and recent memory trends before an OOM");
        yield return (Scraping, $"count by (service) (up{{namespace=\"{ns}\",job=\"kubernetes-service-endpoints\",service=~\"{services}\"}} == 1)",
            "Telemetry metrics endpoint is not being scraped; inspect Service annotations and Prometheus target health");
    }

    async Task<IReadOnlyList<PrometheusSample>> Query(string expression, DateTimeOffset deadline, CheckContext context, CancellationToken cancellationToken) =>
        await prometheus.Query(expression, TimeoutFor(deadline, context), cancellationToken);

    async Task<IReadOnlyList<ScrapeTarget>> Targets(DateTimeOffset deadline, CheckContext context, CancellationToken cancellationToken) =>
        await prometheus.ActiveTargets(TimeoutFor(deadline, context), cancellationToken);

    TimeSpan TimeoutFor(DateTimeOffset deadline, CheckContext context)
    {
        var remaining = deadline - context.TimeProvider.GetUtcNow();
        if (remaining <= TimeSpan.Zero)
        {
            throw new TimeoutException("telemetry query exhausted its check budget or alert-delivery reserve");
        }

        return remaining < _requestTimeout ? remaining : _requestTimeout;
    }

    void Evaluate(string check, string message, IReadOnlyList<PrometheusSample> samples, Dictionary<string, double> limits, Dictionary<string, string> findings)
    {
        var threshold = check switch
        {
            Series => options.SeriesThreshold,
            Memory => 0.9,
            _ => 0
        };
        foreach (var sample in samples.Where(sample => sample.Value > threshold))
        {
            var labels = Labels(sample);
            if (check != Memory)
            {
                findings[$"telemetry:{check}:{labels}"] = $"{message} ({labels}: {General(sample.Value)}; threshold {General(threshold)})";
                continue;
            }

            var container = Required(sample, "container");
            if (options.CacheDominatedContainers.Contains(container))
            {
                continue;
            }

            // The limit comes from a second query; pod churn between the two must not hide the other containers' findings.
            var hasLimit = limits.TryGetValue(Identity(sample), out var limit) && limit != 0;
            var usage = hasLimit ? $"working set {MiB(sample.Value * limit)}MiB of {MiB(limit)}MiB limit " : "working set ";
            var advice = options.TelemetryContainers.Contains(container)
                ? "; inspect ingest rate, queues and recent memory trends"
                : "; inspect this workload's memory use and recent trends";
            findings[$"telemetry:memory:{labels}"] =
                $"Container {Identity(sample)} {usage}({(sample.Value * 100).ToString("F1", CultureInfo.InvariantCulture)}%) exceeds 90%; OOM risk{advice}";
        }
    }
}
