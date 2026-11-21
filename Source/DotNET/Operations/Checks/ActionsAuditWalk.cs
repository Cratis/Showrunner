// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Cratis.Showrunner.Operations.Cluster;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// One walk over the organization's repositories: it lists each repository's workflows, reads the recent scheduled runs of the
/// active ones, and says which workflows are disabled or have stopped running on their own schedule. It stays inside a request and
/// time budget, never follows an address that is not on <c>api.github.com</c>, and never reads a workflow's contents.
/// </summary>
/// <param name="github">The GitHub reader.</param>
/// <param name="options">The options.</param>
/// <param name="timeProvider">The clock.</param>
/// <param name="now">When the audit started, in seconds since the epoch.</param>
/// <param name="deadline">When the walk must be finished.</param>
internal sealed class ActionsAuditWalk(IGitHubReader github, ActionsAuditOptions options, TimeProvider timeProvider, double now, DateTimeOffset deadline)
{
    const string Active = "active";

    static readonly Uri _api = new("https://api.github.com");
    static readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(10);

    int _requests;

    /// <summary>
    /// Gets the time a run was created, in seconds since the epoch.
    /// </summary>
    /// <param name="value">The ISO 8601 text.</param>
    /// <returns>The seconds.</returns>
    internal static double Timestamp(string value) =>
        DateTimeOffset.Parse(value, CultureInfo.InvariantCulture).ToUnixTimeMilliseconds() / 1000.0;

    /// <summary>
    /// Infers how often a workflow is scheduled from the median gap between its latest runs.
    /// </summary>
    /// <param name="runs">The creation time of the runs, most recent first.</param>
    /// <returns>The interval in seconds, or <see langword="null"/> when the history shows fewer than two distinct runs.</returns>
    internal static double? ScheduleInterval(IReadOnlyList<double> runs)
    {
        var moments = runs.Take(10).Distinct().Order().ToList();
        var gaps = moments.Zip(moments.Skip(1), (earlier, later) => later - earlier).Order().ToList();
        if (gaps.Count == 0)
        {
            return null;
        }

        var middle = gaps.Count / 2;
        return (gaps[middle] + gaps[gaps.Count - 1 - middle]) / 2;
    }

    /// <summary>
    /// Walks the organization.
    /// </summary>
    /// <param name="findings">Receives the findings of every repository that could be audited.</param>
    /// <param name="scheduled">The workflows known to be scheduled, as <c>repository/id</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The scheduled workflows to remember and the repositories that were skipped.</returns>
    /// <exception cref="ActionsAuditIncompleteException">The budget ran out; the findings so far are in <paramref name="findings"/>.</exception>
    /// <exception cref="ActionsAuditUnavailableException">Every eligible repository refused the audit.</exception>
    internal async Task<(List<string> Scheduled, HashSet<string> Skipped)> Run(Dictionary<string, string> findings, HashSet<string> scheduled, CancellationToken cancellationToken)
    {
        var skipped = new HashSet<string>(StringComparer.Ordinal);
        var accessible = false;
        await foreach (var repository in Pages($"/orgs/{Uri.EscapeDataString(options.Organization)}/repos?type=all&per_page=100", null, cancellationToken))
        {
            if (IsTrue(repository, "archived") || IsTrue(repository, "fork") || IsTrue(repository, "disabled"))
            {
                continue;
            }

            var name = repository.GetProperty("full_name").GetString()!;
            var repositoryFindings = new Dictionary<string, string>(StringComparer.Ordinal);
            try
            {
                scheduled = await AuditRepository(name, scheduled, repositoryFindings, cancellationToken);
            }
            catch (GitHubApiException error) when (Inaccessible(error))
            {
                skipped.Add(name);
                continue;
            }
            catch (ActionsAuditIncompleteException)
            {
                Merge(findings, repositoryFindings);
                throw;
            }

            Merge(findings, repositoryFindings);
            accessible = true;
        }

        if (skipped.Count > 0 && !accessible)
        {
            throw new ActionsAuditUnavailableException();
        }

        var kept = scheduled.Where(key => skipped.Contains(key[..key.LastIndexOf('/')])).Order(StringComparer.Ordinal).ToList();
        return ([.. kept.Concat(scheduled.Except(kept).Order(StringComparer.Ordinal)).Take(options.MaximumRememberedSchedules)], skipped);
    }

    static bool IsTrue(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;

    static void Merge(Dictionary<string, string> target, Dictionary<string, string> source)
    {
        foreach (var (key, text) in source)
        {
            target[key] = text;
        }
    }

    static bool Inaccessible(GitHubApiException error) => error.StatusCode == 404 || (error.StatusCode == 403 && !error.IsRateLimited);

    static string Quote(string name) => Uri.EscapeDataString(name).Replace("%2F", "/", StringComparison.Ordinal);

    Dictionary<string, string> WorkflowFindings(string repository, JsonElement workflow, IReadOnlyList<double> runs)
    {
        var path = workflow.GetProperty("path").GetString();
        var state = workflow.GetProperty("state").GetString();
        if (state != Active)
        {
            var updated = workflow.TryGetProperty("updated_at", out var at) && at.ValueKind == JsonValueKind.String ? at.GetString() : "unknown";
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [$"actions-audit:disabled:{repository}:{path}"] = $"GitHub workflow {repository}/{path}: {state}; updated_at {updated}; interval unknown"
            };
        }

        // History cannot identify schedules that never fired or have only fired once.
        if (ScheduleInterval(runs) is not { } interval)
        {
            return [];
        }

        var age = now - runs.Take(10).Max();
        return age > Math.Max(2 * interval, 3600)
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [$"actions-audit:stalled:{repository}:{path}"] = $"GitHub workflow {repository}/{path}: last scheduled run {(long)age}s ago; inferred interval {(long)interval}s"
            }
            : [];
    }

    async Task<GitHubPage> Get(string path, CancellationToken cancellationToken)
    {
        var uri = new Uri(_api, path);
        if (uri.Scheme != Uri.UriSchemeHttps || !string.Equals(uri.Host, _api.Host, StringComparison.Ordinal))
        {
            // Never send the credential anywhere but GitHub's API.
            throw new InvalidOperationException("untrusted pagination");
        }

        var remaining = deadline - timeProvider.GetUtcNow();
        if (_requests >= options.MaximumRequests || remaining <= TimeSpan.Zero)
        {
            throw new ActionsAuditIncompleteException();
        }

        _requests++;
        try
        {
            var page = await github.Get(uri, remaining < _requestTimeout ? remaining : _requestTimeout, cancellationToken);
            return timeProvider.GetUtcNow() >= deadline ? throw new ActionsAuditIncompleteException() : page;
        }
        catch (GitHubResponseTooLargeException)
        {
            throw new ActionsAuditIncompleteException();
        }
    }

    async IAsyncEnumerable<JsonElement> Pages(string path, string? field, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var next = path;
        while (next is not null)
        {
            var page = await Get(next, cancellationToken);
            var items = field is null ? page.Body : page.Body.GetProperty(field);
            if (items.GetArrayLength() == 0)
            {
                yield break;
            }

            foreach (var item in items.EnumerateArray())
            {
                yield return item;
            }

            next = page.Next;
        }
    }

    async Task<Dictionary<long, List<double>>> RepositoryRuns(string repository, List<JsonElement> workflows, CancellationToken cancellationToken)
    {
        // Actions-off and disabled-only repositories need no run calls.
        if (!workflows.Exists(workflow => workflow.GetProperty("state").GetString() == Active))
        {
            return [];
        }

        var page = await Get($"/repos/{repository}/actions/runs?event=schedule&per_page=100", cancellationToken);
        return page.Body.GetProperty("workflow_runs").EnumerateArray()
            .GroupBy(run => run.GetProperty("workflow_id").GetInt64())
            .ToDictionary(
                group => group.Key,
                group => group.Select(run => Timestamp(run.GetProperty("created_at").GetString()!)).OrderDescending().Take(10).ToList());
    }

    async Task<HashSet<string>> AuditRepository(string name, HashSet<string> scheduled, Dictionary<string, string> findings, CancellationToken cancellationToken)
    {
        var repository = Quote(name);
        var workflows = new List<JsonElement>();
        await foreach (var workflow in Pages($"/repos/{repository}/actions/workflows?per_page=100", "workflows", cancellationToken))
        {
            workflows.Add(workflow);
        }

        var existing = workflows.Select(workflow => $"{name}/{workflow.GetProperty("id").GetInt64()}").ToHashSet(StringComparer.Ordinal);
        var retained = scheduled.Where(key => !key.StartsWith(name + "/", StringComparison.Ordinal)).Concat(scheduled.Where(existing.Contains)).ToHashSet(StringComparer.Ordinal);
        foreach (var workflow in workflows.Where(workflow => workflow.GetProperty("state").GetString() != Active))
        {
            Merge(findings, WorkflowFindings(name, workflow, []));
        }

        var grouped = await RepositoryRuns(repository, workflows, cancellationToken);
        foreach (var workflow in workflows.Where(workflow => workflow.GetProperty("state").GetString() == Active))
        {
            var id = workflow.GetProperty("id").GetInt64();
            var runs = grouped.TryGetValue(id, out var known) ? known : [];
            var key = $"{name}/{id}";
            if (runs.Count > 0)
            {
                retained.Add(key);
            }

            if (retained.Contains(key) && runs.Count < 2)
            {
                var page = await Get($"/repos/{repository}/actions/workflows/{id}/runs?event=schedule&per_page=10", cancellationToken);
                runs = [.. page.Body.GetProperty("workflow_runs").EnumerateArray().Select(run => Timestamp(run.GetProperty("created_at").GetString()!))];
            }

            Merge(findings, WorkflowFindings(name, workflow, runs));
        }

        return retained;
    }
}
