// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Cratis.Showrunner.Operations.State;

/// <summary>
/// Reads and writes <see cref="AlertStateSnapshot"/> in the format the Python watchdog kept in its ConfigMap, so a deployment
/// that moves to this watchdog keeps its cooldowns and open-since times: finding keys map to the epoch seconds they were last
/// alerted, next to the reserved <c>__last_check</c>, <c>__first_seen</c>, <c>__summary_streaks</c>,
/// <c>__reported_failed_jobs</c> and <c>__actions_audit</c> entries.
/// </summary>
public static class AlertStateJson
{
    const string LastCheck = "__last_check";
    const string FirstSeen = "__first_seen";
    const string SummaryStreaks = "__summary_streaks";
    const string ReportedFailedJobs = "__reported_failed_jobs";
    const string ActionsAudit = "__actions_audit";

    /// <summary>
    /// Parses the stored JSON.
    /// </summary>
    /// <param name="json">The stored JSON; blank means no state.</param>
    /// <returns>The state.</returns>
    public static AlertStateSnapshot Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || JsonNode.Parse(json) is not JsonObject root)
        {
            return AlertStateSnapshot.Empty;
        }

        var lastAlerted = root
            .Where(entry => !entry.Key.StartsWith("__", StringComparison.Ordinal) && Seconds(entry.Value) is not null)
            .ToImmutableDictionary(entry => entry.Key, entry => FromSeconds(Seconds(entry.Value)!.Value));

        var firstSeen = (root[FirstSeen] as JsonObject)?
            .Where(entry => Seconds(entry.Value) is not null)
            .ToImmutableDictionary(entry => entry.Key, entry => FromSeconds(Seconds(entry.Value)!.Value))
            ?? ImmutableDictionary<string, DateTimeOffset>.Empty;

        var streaks = (root[SummaryStreaks] as JsonObject)?
            .Where(entry => entry.Value is JsonValue value && value.TryGetValue<int>(out _))
            .ToImmutableDictionary(entry => entry.Key, entry => entry.Value!.GetValue<int>())
            ?? ImmutableDictionary<string, int>.Empty;

        var reported = (root[ReportedFailedJobs] as JsonArray)?
            .Select(node => node is JsonValue value && value.TryGetValue<string>(out var name) ? name : null)
            .OfType<string>()
            .ToImmutableList()
            ?? [];

        return new AlertStateSnapshot(
            lastAlerted,
            firstSeen,
            Seconds(root[LastCheck]) is { } last ? FromSeconds(last) : null,
            streaks,
            reported,
            root[ActionsAudit]?.ToJsonString());
    }

    /// <summary>
    /// Serializes the state.
    /// </summary>
    /// <param name="snapshot">The state.</param>
    /// <returns>The JSON to store.</returns>
    public static string Serialize(AlertStateSnapshot snapshot)
    {
        var root = new JsonObject();
        foreach (var (key, at) in snapshot.LastAlerted)
        {
            root[key] = ToSeconds(at);
        }

        if (snapshot.LastCheck is { } last)
        {
            root[LastCheck] = ToSeconds(last);
        }

        root[SummaryStreaks] = new JsonObject(snapshot.SummaryStreaks.Select(entry => KeyValuePair.Create(entry.Key, (JsonNode?)JsonValue.Create(entry.Value))));
        root[FirstSeen] = new JsonObject(snapshot.FirstSeen.Select(entry => KeyValuePair.Create(entry.Key, (JsonNode?)JsonValue.Create(ToSeconds(entry.Value)))));
        root[ReportedFailedJobs] = new JsonArray([.. snapshot.ReportedFailedJobs.Order(StringComparer.Ordinal).Select(name => (JsonNode?)JsonValue.Create(name))]);
        root[ActionsAudit] = snapshot.ActionsAudit is null ? new JsonObject() : JsonNode.Parse(snapshot.ActionsAudit);
        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    }

    static double? Seconds(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<double>(out var seconds) ? seconds : null;

    static DateTimeOffset FromSeconds(double seconds) => DateTimeOffset.FromUnixTimeMilliseconds((long)(seconds * 1000));

    static double ToSeconds(DateTimeOffset at) => double.Parse(
        (at.ToUnixTimeMilliseconds() / 1000.0).ToString("0.###", CultureInfo.InvariantCulture),
        CultureInfo.InvariantCulture);
}
