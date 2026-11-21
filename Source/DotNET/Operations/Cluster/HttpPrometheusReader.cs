// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Text.Json;

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// Reads Prometheus over its HTTP API. Anything that is not a clean success is an exception: a Prometheus that answers with
/// warnings, a result that is not a vector or a sample that cannot be read must never be taken as a healthy cluster.
/// </summary>
/// <param name="http">The HTTP client.</param>
/// <param name="baseUri">The Prometheus base address.</param>
public sealed class HttpPrometheusReader(HttpClient http, Uri baseUri) : IPrometheusReader
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PrometheusSample>> Query(string expression, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var data = await Get($"/api/v1/query?query={Uri.EscapeDataString(expression)}", timeout, cancellationToken);
        var root = data.RootElement;
        if (!root.TryGetProperty("resultType", out var type) || type.GetString() != "vector" || !root.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("query did not return an instant vector");
        }

        var samples = new List<PrometheusSample>();
        foreach (var item in result.EnumerateArray())
        {
            samples.Add(Sample(item));
        }

        return samples;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScrapeTarget>> ActiveTargets(TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var data = await Get("/api/v1/targets?state=active", timeout, cancellationToken);
        if (!data.RootElement.TryGetProperty("activeTargets", out var targets) || targets.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("target query did not return active targets");
        }

        var result = new List<ScrapeTarget>();
        foreach (var target in targets.EnumerateArray())
        {
            if (!target.TryGetProperty("labels", out var labels) || labels.ValueKind != JsonValueKind.Object
                || !target.TryGetProperty("lastError", out var error) || error.ValueKind != JsonValueKind.String
                || !target.TryGetProperty("health", out var health) || health.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException("target query returned unreadable health");
            }

            result.Add(new ScrapeTarget(Label(labels, "job"), Label(labels, "instance"), health.GetString()!, error.GetString()!));
        }

        return result;
    }

    static PrometheusSample Sample(JsonElement item)
    {
        const string unreadable = "query returned an unreadable sample";
        if (!item.TryGetProperty("metric", out var metric) || metric.ValueKind != JsonValueKind.Object
            || !item.TryGetProperty("value", out var value) || value.ValueKind != JsonValueKind.Array || value.GetArrayLength() != 2)
        {
            throw new InvalidOperationException(unreadable);
        }

        var labels = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in metric.EnumerateObject())
        {
            labels[property.Name] = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString()! : throw new InvalidOperationException(unreadable);
        }

        var timestamp = value[0].ValueKind == JsonValueKind.Number ? value[0].GetDouble() : double.NaN;
        var text = value[1].ValueKind == JsonValueKind.String ? value[1].GetString() : null;
        if (!double.IsFinite(timestamp) || !double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) || !double.IsFinite(number))
        {
            throw new InvalidOperationException(unreadable);
        }

        return new PrometheusSample(labels, number);
    }

    static string Label(JsonElement labels, string name) =>
        labels.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString()! : string.Empty;

    async Task<JsonDocument> Get(string path, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limit.CancelAfter(timeout);
        using var response = await http.GetAsync(new Uri(baseUri, path), limit.Token);
        response.EnsureSuccessStatusCode();
        var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(limit.Token), cancellationToken: limit.Token);
        try
        {
            var root = body.RootElement;
            var succeeded = root.TryGetProperty("status", out var status) && status.GetString() == "success";
            var warned = root.TryGetProperty("warnings", out var warnings) && warnings.ValueKind == JsonValueKind.Array && warnings.GetArrayLength() > 0;
            if (!succeeded || warned || !root.TryGetProperty("data", out _))
            {
                var error = root.TryGetProperty("error", out var message) && message.ValueKind == JsonValueKind.String ? message.GetString() : null;
                throw new InvalidOperationException(error ?? (warned ? string.Join("; ", warnings.EnumerateArray().Select(warning => warning.GetString())) : "query did not succeed"));
            }

            return JsonDocument.Parse(root.GetProperty("data").GetRawText());
        }
        finally
        {
            body.Dispose();
        }
    }
}
