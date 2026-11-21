// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Text.Json.Nodes;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Audits the GitHub Actions schedules of an organization with read-only history and no workflow contents permission: a workflow
/// that is disabled, or whose scheduled runs have stopped for more than twice their inferred interval (at least an hour), is a finding.
/// GitHub is asked at most once a day; in between, and while GitHub rate limits the audit, the findings of the last complete audit
/// are repeated so they keep their open-since time. An incomplete or failed audit never resolves what it did not look at. The
/// failure text never carries a response body, a URL or the credential.
/// </summary>
/// <param name="github">The GitHub reader.</param>
/// <param name="options">The options.</param>
public sealed class ActionsScheduleAuditCheck(IGitHubReader github, ActionsAuditOptions options) : IPersistentCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "actions-audit";

    const double Day = 24 * 3600;
    const double Hour = 3600;

    JsonObject _state = [];

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public void Restore(string? json) => _state = json is null ? [] : JsonNode.Parse(json) as JsonObject ?? [];

    /// <inheritdoc/>
    public string? Persist() => _state.Count == 0 ? null : _state.ToJsonString();

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        var now = context.Now.ToUnixTimeMilliseconds() / 1000.0;
        if (now < Number("retry_at") || now - Number("checked_at") < Day)
        {
            return Cached();
        }

        var findings = new Dictionary<string, string>(StringComparer.Ordinal);
        _state["last_attempt"] = now;
        var retryAt = now + Day;
        var deadline = context.TimeProvider.GetUtcNow() + options.Budget;
        if (deadline > context.CollectionDeadline)
        {
            deadline = context.CollectionDeadline;
        }

        var walk = new ActionsAuditWalk(github, options, context.TimeProvider, now, deadline);
        try
        {
            var known = (_state["scheduled"] as JsonArray)?.Select(key => key?.GetValue<string>()).OfType<string>().ToHashSet(StringComparer.Ordinal) ?? [];
            var (scheduled, skipped) = await walk.Run(findings, known, cancellationToken);
            RetainSkippedFindings(findings, skipped);
            _state["checked_at"] = now;
            _state["findings"] = ToJson(findings);
            _state["scheduled"] = new JsonArray([.. scheduled.Select(key => (JsonNode)JsonValue.Create(key))]);
            _state["skipped_repos"] = skipped.Count;
        }
        catch (ActionsAuditIncompleteException)
        {
            findings["actions-audit:incomplete"] = "GitHub Actions audit incomplete: collection budget exhausted";
        }
        catch (ActionsAuditUnavailableException)
        {
            findings["actions-audit:unavailable"] = "GitHub Actions audit unavailable: no repositories accessible";
            retryAt = now + Hour;
        }
        catch (GitHubApiException error)
        {
            findings["actions-audit:unavailable"] = $"GitHub Actions audit unavailable: HTTP {error.StatusCode}";
            retryAt = error.StatusCode is 403 or 429 ? RateLimitRetry(error, now) : now + Hour;
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            // Never include exception bodies, URLs or credentials.
            findings["actions-audit:unavailable"] = "GitHub Actions audit unavailable: request or response failed";
            retryAt = now + Hour;
        }

        // An incomplete scan is not proof that previous findings have resolved.
        var merged = FindingsOf(_state["findings"]);
        foreach (var (key, text) in findings)
        {
            merged[key] = text;
        }

        _state["findings"] = ToJson(merged);
        _state["retry_at"] = retryAt;
        return Cached();
    }

    static JsonObject ToJson(Dictionary<string, string> findings)
    {
        var json = new JsonObject();
        foreach (var (key, text) in findings)
        {
            json[key] = text;
        }

        return json;
    }

    static Dictionary<string, string> FindingsOf(JsonNode? node)
    {
        var findings = new Dictionary<string, string>(StringComparer.Ordinal);
        if (node is JsonObject json)
        {
            foreach (var (key, value) in json)
            {
                if (value?.GetValue<string>() is { } text)
                {
                    findings[key] = text;
                }
            }
        }

        return findings;
    }

    static double RateLimitRetry(GitHubApiException error, double now)
    {
        var retryAt = now + Day;
        if (double.TryParse(error.RetryAfter, NumberStyles.Integer, CultureInfo.InvariantCulture, out var after))
        {
            retryAt = Math.Max(retryAt, now + after);
        }

        if (double.TryParse(error.RateLimitReset, NumberStyles.Integer, CultureInfo.InvariantCulture, out var reset))
        {
            retryAt = Math.Max(retryAt, reset);
        }

        return retryAt;
    }

    double Number(string name) => _state[name] is JsonValue value && value.TryGetValue<double>(out var number) ? number : 0;

    void RetainSkippedFindings(Dictionary<string, string> findings, HashSet<string> skipped)
    {
        foreach (var (key, text) in FindingsOf(_state["findings"]))
        {
            var parts = key.Split(':', 4);
            if (parts.Length == 4 && parts[0] == "actions-audit" && skipped.Contains(parts[2]))
            {
                findings[key] = text;
            }
        }
    }

    IReadOnlyCollection<Finding> Cached() => [.. FindingsOf(_state["findings"]).Select(entry => new Finding(entry.Key, entry.Value))];
}
