// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;
using Cratis.Showrunner.Operations.State;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Keeps a GitHub Actions runner pool from filling up with hung work. A job that has held a runner for the maximum time is stopped
/// by deleting its ephemeral runner (the scale set creates a fresh one), and a runner that has no job and never got past starting is
/// replaced because it is stuck on its scratch volume or on scheduling. The time is measured from the first time the runner was seen
/// holding a job, never from the age of the pod, so a job is stopped a little late and never early. At most <see cref="RunnerPoolOptions.Cap"/>
/// runners are stopped or replaced per pass; more over the limit than that stops nothing and is a critical finding. Everything it does
/// is a finding, so the same alert reaches every channel.
/// </summary>
/// <param name="runners">The runner scale set resources.</param>
/// <param name="state">The ConfigMap store the first-seen times are kept in.</param>
/// <param name="options">The options.</param>
public sealed class RunnerPoolCheck(IRunnerCluster runners, IConfigMapStore state, RunnerPoolOptions options) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "runners";

    const string BusyKey = "busy";

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        var now = context.Now.ToUnixTimeMilliseconds() / 1000.0;
        var pool = (await runners.ListRunners(options.Namespace, cancellationToken)).Where(runner => options.ScaleSets.Contains(runner.ScaleSet)).ToList();
        var previous = await LoadState(cancellationToken);
        var seen = new Dictionary<string, double>(StringComparer.Ordinal);
        var over = new List<(int Minutes, string Name, string Line)>();
        var pending = new List<(string Name, string Line)>();
        foreach (var runner in pool)
        {
            if (!runner.HasJob)
            {
                var waited = (now - (runner.Created.ToUnixTimeMilliseconds() / 1000.0)) / 60;
                if (runner.Phase == "Pending" && waited > options.PendingTime.TotalMinutes)
                {
                    pending.Add((runner.Name, $"{runner.Name} (pending {(int)waited} min)"));
                }

                continue;
            }

            // The first time this runner was seen holding a job, never earlier than the real start.
            seen[runner.Name] = previous.TryGetValue(runner.Name, out var first) ? first : now;
            var minutes = (int)((now - seen[runner.Name]) / 60);
            if (options.MaximumJobTime > TimeSpan.Zero && minutes >= options.MaximumJobTime.TotalMinutes)
            {
                over.Add((minutes, runner.Name, Describe(runner, minutes)));
            }
        }

        // The state is saved before anything is stopped, so a failed stop never loses when a job was first seen.
        await SaveState(seen, cancellationToken);
        var findings = new List<Finding>();
        await ReplacePending(pending, findings, cancellationToken);
        if (over.Count > options.Cap)
        {
            var lines = string.Join("; ", over.OrderByDescending(entry => entry.Minutes).ThenByDescending(entry => entry.Name, StringComparer.Ordinal).Select(entry => entry.Line));
            var text = $"{over.Count} runners are over the limit, more than the {options.Cap} one pass may stop. Nothing was stopped; inspect GitHub and the cluster clock before acting. Over the limit: {lines}";
            findings.Add(new Finding("runners:refused", text, AlertSeverity.Critical));
            return findings;
        }

        foreach (var (_, name, line) in over.OrderByDescending(entry => entry.Minutes).ThenByDescending(entry => entry.Name, StringComparer.Ordinal))
        {
            await Delete(name, cancellationToken);
            findings.Add(new Finding($"runners:stopped:{name}", $"stopped runner {line}: the job ran past the {(int)options.MaximumJobTime.TotalMinutes} minute limit"));
        }

        return findings;
    }

    static string Describe(RunnerInfo runner, int minutes) =>
        $"{runner.Name} ({runner.Repository ?? "unknown repository"}, {runner.JobName ?? "unknown job"}; job assigned for {minutes} min)";

    async Task ReplacePending(List<(string Name, string Line)> pending, List<Finding> findings, CancellationToken cancellationToken)
    {
        // A runner with no job that never got past starting holds no work, so replacing it loses nothing. At most the cap per pass,
        // so a cluster-wide scheduling wait cannot turn into a delete storm.
        foreach (var (name, line) in pending.Take(options.Cap))
        {
            await Delete(name, cancellationToken);
            findings.Add(new Finding($"runners:replaced:{name}", $"replaced runner {line}: no job assigned, so a fresh runner and volume are created", AlertSeverity.Information));
        }

        foreach (var (name, line) in pending.Skip(options.Cap))
        {
            findings.Add(new Finding($"runners:pending:{name}", $"runner {line} is still pending; it is over the per-pass replacement cap and is left for the next pass"));
        }
    }

    async Task Delete(string name, CancellationToken cancellationToken)
    {
        try
        {
            await runners.DeleteRunner(options.Namespace, name, cancellationToken);
        }
        catch (ClusterReadException error) when (error.StatusCode == 404)
        {
            // Already gone is what was wanted.
        }
    }

    async Task<Dictionary<string, double>> LoadState(CancellationToken cancellationToken)
    {
        var busy = new Dictionary<string, double>(StringComparer.Ordinal);
        if (await state.Read(options.StateConfigMap, BusyKey, cancellationToken) is { } json && JsonNode.Parse(json) is JsonObject saved)
        {
            foreach (var (name, value) in saved)
            {
                if (value is JsonValue number && number.TryGetValue<double>(out var seconds))
                {
                    busy[name] = seconds;
                }
            }
        }

        return busy;
    }

    async Task SaveState(Dictionary<string, double> seen, CancellationToken cancellationToken)
    {
        var json = new JsonObject();
        foreach (var (name, seconds) in seen.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            json[name] = seconds;
        }

        await state.Write(options.StateConfigMap, BusyKey, json.ToJsonString(), cancellationToken);
    }
}
