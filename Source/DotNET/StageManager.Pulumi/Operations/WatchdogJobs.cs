// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Text.Json;
using Pulumi;
using Pulumi.Kubernetes.Types.Inputs.Rbac.V1;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// Plans the CronJobs of the job kinds of a <see cref="Watchdog"/>: what each runs, the variables it gets, the Secret values it
/// needs and the rights it is granted. A job is only planned when its inputs are given, and it is granted only what it does.
/// </summary>
internal static class WatchdogJobs
{
    /// <summary>The runner pool job.</summary>
    internal const string Runners = "runners";

    /// <summary>The agent harness job.</summary>
    internal const string AgentHarness = "agent-harness";

    /// <summary>The orphaned volume sweep job.</summary>
    internal const string Volumes = "volumes";

    /// <summary>The synthetic probe job.</summary>
    internal const string Probe = "probe";

    /// <summary>The key of the UpCloud user in the job's Secret.</summary>
    internal const string UpCloudUsernameKey = "UPCLOUD_USERNAME";

    /// <summary>The key of the UpCloud password in the job's Secret.</summary>
    internal const string UpCloudPasswordKey = "UPCLOUD_PASSWORD";

    static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };

    /// <summary>
    /// Plans the jobs.
    /// </summary>
    /// <param name="kubernetesName">The Kubernetes name of the watchdog.</param>
    /// <param name="args">The inputs.</param>
    /// <returns>The jobs that are configured, in a stable order.</returns>
    internal static IReadOnlyList<WatchdogJob> Plan(string kubernetesName, WatchdogArgs args)
    {
        var jobs = new List<WatchdogJob>();
        if (args.Runners is { } runners)
        {
            jobs.Add(RunnerJob(kubernetesName, runners));
        }

        if (args.AgentHarness is { } harness)
        {
            jobs.Add(AgentHarnessJob(harness));
        }

        if (args.VolumeSweeper is { } sweeper)
        {
            jobs.Add(VolumeJob(sweeper));
        }

        if (args.Probe is { } probe)
        {
            jobs.Add(ProbeJob(probe));
        }

        return jobs;
    }

    static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    static PolicyRuleArgs Rule(string group, string resource, params string[] verbs) => new() { ApiGroups = [group], Resources = [resource], Verbs = verbs };

    static Dictionary<string, IReadOnlyList<PolicyRuleArgs>> In(string @namespace, params PolicyRuleArgs[] rules) =>
        new(StringComparer.Ordinal) { [@namespace] = rules };

    static WatchdogJob RunnerJob(string kubernetesName, WatchdogRunnersArgs runners)
    {
        var state = $"{kubernetesName}-runners-state";
        List<KeyValuePair<string, string>> environment =
        [
            new("RUNNER_NAMESPACE", runners.Namespace),
            new("RUNNER_SCALE_SETS", string.Join(',', runners.ScaleSets)),
            new("RUNNER_STATE_CONFIGMAP", state),
            new("WATCHDOG_CHECKS", "runners")
        ];
        if (runners.MaximumJobMinutes is { } maximum)
        {
            environment.Add(new("MAX_JOB_MINUTES", Number(maximum)));
        }

        if (runners.PendingMinutes is { } pending)
        {
            environment.Add(new("PENDING_MINUTES", Number(pending)));
        }

        return new WatchdogJob(
            Runners,
            "runners",
            runners.Schedule ?? "*/5 * * * *",
            environment,
            new Dictionary<string, Input<string>>(),
            [],
            In(runners.Namespace, Rule("actions.github.com", "ephemeralrunners", "list", "delete")),
            [state]);
    }

    static WatchdogJob AgentHarnessJob(WatchdogAgentHarnessArgs harness) => new(
        AgentHarness,
        "agent-harness",
        harness.Schedule ?? "*/5 * * * *",
        [new("AGENT_NAMESPACE", harness.Namespace), new("CLEANUP_ENABLED", harness.CleanupEnabled ? "true" : "false"), new("WATCHDOG_CHECKS", "agent-harness")],
        new Dictionary<string, Input<string>>(),
        [],
        In(
            harness.Namespace,
            Rule(string.Empty, "pods", "list"),
            Rule(string.Empty, "events", "list"),

            // Only whether a Secret exists is asked, with a request for its metadata; the data is never read or kept.
            Rule(string.Empty, "secrets", "get"),
            Rule("batch", "jobs", "get", "list", "delete")),
        []);

    static WatchdogJob VolumeJob(WatchdogVolumeSweeperArgs sweeper)
    {
        List<KeyValuePair<string, string>> environment = [new("DRY_RUN", sweeper.DryRun ? "true" : "false"), new("WATCHDOG_CHECKS", "volume-sweeper")];
        if (sweeper.MinimumAgeSeconds is { } age)
        {
            environment.Add(new("MIN_AGE_SECONDS", Number(age)));
        }

        if (sweeper.MaximumPerRun is { } most)
        {
            environment.Add(new("MAX_PER_RUN", Number(most)));
        }

        return new WatchdogJob(
            Volumes,
            "volume-sweeper",
            sweeper.Schedule ?? "0 * * * *",
            environment,
            new Dictionary<string, Input<string>> { [UpCloudUsernameKey] = sweeper.Username, [UpCloudPasswordKey] = sweeper.Password },
            [Rule(string.Empty, "persistentvolumes", "list")],
            new Dictionary<string, IReadOnlyList<PolicyRuleArgs>>(),
            []);
    }

    static WatchdogJob ProbeJob(WatchdogProbeArgs probe)
    {
        var targets = probe.Targets.Select(target => new
        {
            name = target.Name,
            baseUrl = target.BaseUrl,
            checks = target.Checks.Select(check => new
            {
                id = check.Id,
                method = check.Method,
                path = check.Path,
                accept = check.Accept,
                body = check.Body,
                expectStatus = check.ExpectStatus,
                timeoutMs = check.TimeoutMilliseconds
            })
        });
        List<KeyValuePair<string, string>> environment = [new("PROBE_TARGETS", JsonSerializer.Serialize(targets, _json)), new("WATCHDOG_CHECKS", "probe")];
        if (probe.RoundGapSeconds is { } gap)
        {
            environment.Add(new("PROBE_ROUND_GAP_SECONDS", Number(gap)));
        }

        return new WatchdogJob(Probe, "probe", probe.Schedule ?? "*/5 * * * *", environment, new Dictionary<string, Input<string>>(), [], new Dictionary<string, IReadOnlyList<PolicyRuleArgs>>(), []);
    }
}
