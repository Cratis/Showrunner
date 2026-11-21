// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Finds agent worker Jobs that can never start because a Secret their Pod mounts does not exist, and, when cleanup is enabled,
/// deletes them. Deletion is the last step of a chain of fences: the Job and every Pod it owns must look like an untouched,
/// scoped worker that never executed; the missing Secret must be confirmed by recent repeated <c>FailedMount</c> events; and the
/// Job and its Pods are read again, twice, immediately before a foreground delete that carries the Job's UID and resource
/// version as preconditions. Missing evidence is "unknown", never permission: an unknown outcome stops the rest of the cleanup.
/// At most <see cref="AgentHarnessOptions.Cap"/> Jobs are deleted per pass. It reports counts only, never bodies, errors or credentials.
/// </summary>
/// <param name="cluster">The cluster operations.</param>
/// <param name="options">The options.</param>
public sealed class AgentHarnessCheck(IAgentHarnessCluster cluster, AgentHarnessOptions options) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "agent-harness";

    IReadOnlyCollection<Finding>? _acted;

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        // The pipeline asks again to confirm a finding. A delete is an action, so it is done once per run and the confirmation sees the same result.
        return _acted ??= await Act(context, cancellationToken);
    }

    /// <summary>
    /// Runs one pass.
    /// </summary>
    /// <param name="time">The clock.</param>
    /// <param name="deadline">When the pass must have finished.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>What the pass counted.</returns>
    public async Task<AgentHarnessCounts> Scan(TimeProvider time, DateTimeOffset deadline, CancellationToken cancellationToken)
    {
        var tally = new Tally();
        var candidates = new List<(AgentJob Job, IReadOnlyList<AgentPod> Pods)>();
        try
        {
            var jobs = await cluster.ListJobs(options.Namespace, options.JobLabels, cancellationToken);
            foreach (var job in jobs)
            {
                CheckDeadline(time, deadline);
                tally.Considered++;
                try
                {
                    var pods = await cluster.PodsOwnedBy(options.Namespace, job.Uid, cancellationToken);
                    tally.Running += pods.Any(pod => pod.Phase == "Running") ? 1 : 0;
                    tally.Exited += pods.Any(pod => pod.ExitedNonZero) ? 1 : 0;
                    if (await Eligible(job, pods, time, cancellationToken))
                    {
                        candidates.Add((job, pods));
                    }
                    else
                    {
                        tally.Protected++;
                    }
                }
                catch (Exception error) when (error is not OperationCanceledException)
                {
                    tally.Unknown++;
                    tally.Protected++;
                }
            }

            tally.Candidates = candidates.Count;
            await Clean(candidates, tally, time, deadline, cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            tally.Unknown++;
        }

        return tally.ToCounts();
    }

    static void CheckDeadline(TimeProvider time, DateTimeOffset deadline)
    {
        if (time.GetUtcNow() >= deadline)
        {
            throw new TimeoutException("the pass ran out of time");
        }
    }

    static bool SameVersions(IReadOnlyList<AgentPod> first, IReadOnlyList<AgentPod> second) =>
        first.Select(pod => pod.Fingerprint).SequenceEqual(second.Select(pod => pod.Fingerprint), StringComparer.Ordinal);

    static string Summary(AgentHarnessCounts counts) =>
        $"considered {counts.Considered}, protected {counts.Protected}, candidates {counts.Candidates}, deleted {counts.Deleted}, unknown {counts.Unknown}, running {counts.Running}, exited non-zero {counts.Exited}";

    static bool HasLabels(IReadOnlyDictionary<string, string> actual, IReadOnlyDictionary<string, string> expected) =>
        expected.All(label => actual.TryGetValue(label.Key, out var value) && value == label.Value);

    static bool IsEvidence(AgentEvent evidence, AgentPod pod, string secret, DateTimeOffset now, TimeSpan recent) =>
        evidence.InvolvedUid == pod.Uid
        && evidence.InvolvedKind == "Pod"
        && evidence.Reason == "FailedMount"
        && evidence.Count >= 2
        && evidence.Latest is { } latest
        && now - latest >= TimeSpan.Zero
        && now - latest <= recent
        && MissingSecretMessage(secret).IsMatch(evidence.Message);

    static Regex MissingSecretMessage(string secret) =>
        new($"secret [\"']{Regex.Escape(secret)}[\"'] not found", RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));

    async Task<IReadOnlyCollection<Finding>> Act(CheckContext context, CancellationToken cancellationToken)
    {
        var deadline = context.TimeProvider.GetUtcNow() + options.Budget;
        if (deadline > context.CollectionDeadline)
        {
            deadline = context.CollectionDeadline;
        }

        var counts = await Scan(context.TimeProvider, deadline, cancellationToken);
        var findings = new List<Finding>();
        if (counts.Unknown > 0)
        {
            findings.Add(new Finding(
                "agent-harness:unknown",
                $"agent harness watchdog could not verify {counts.Unknown} item(s) in {options.Namespace}; nothing is deleted without evidence. {Summary(counts)}",
                AlertSeverity.Critical));
        }

        if (counts.Failed > 0)
        {
            findings.Add(new Finding(
                "agent-harness:refused",
                $"agent harness cleanup refused: more than {options.Cap} Jobs were candidates, or a Job changed while it was being verified. Nothing was deleted for them. {Summary(counts)}"));
        }

        if (counts.Candidates > 0 && !options.CleanupEnabled)
        {
            findings.Add(new Finding(
                "agent-harness:candidates",
                $"{counts.Candidates} agent worker Job(s) in {options.Namespace} can never start because a required Secret is missing; cleanup is disabled. {Summary(counts)}"));
        }

        if (counts.Deleted > 0)
        {
            findings.Add(new Finding(
                "agent-harness:deleted",
                $"deleted {counts.Deleted} agent worker Job(s) in {options.Namespace} that could never start because a required Secret is missing. {Summary(counts)}",
                AlertSeverity.Information));
        }

        return findings;
    }

    async Task Clean(List<(AgentJob Job, IReadOnlyList<AgentPod> Pods)> candidates, Tally tally, TimeProvider time, DateTimeOffset deadline, CancellationToken cancellationToken)
    {
        if (candidates.Count > options.Cap)
        {
            tally.Failed++;
            tally.Protected += candidates.Count;
            return;
        }

        if (tally.Unknown > 0 || !options.CleanupEnabled)
        {
            tally.Protected += candidates.Count;
            return;
        }

        for (var index = 0; index < candidates.Count; index++)
        {
            if (tally.Unknown > 0)
            {
                // An unknown outcome stops the remaining cleanup.
                tally.Protected += candidates.Count - index;
                return;
            }

            CheckDeadline(time, deadline);
            await CleanOne(candidates[index].Job, candidates[index].Pods, tally, time, cancellationToken);
        }
    }

    async Task CleanOne(AgentJob job, IReadOnlyList<AgentPod> pods, Tally tally, TimeProvider time, CancellationToken cancellationToken)
    {
        try
        {
            var current = await cluster.ReadJob(options.Namespace, job.Name, cancellationToken);
            var fresh = await cluster.PodsOwnedBy(options.Namespace, job.Uid, cancellationToken);
            if (current.Fingerprint != job.Fingerprint || !SameVersions(fresh, pods) || !await Eligible(current, fresh, time, cancellationToken))
            {
                tally.Protected++;
                return;
            }

            // The evidence reads may outlive a restored Secret: fence every owned Pod again after them, not just the Job.
            var latest = await cluster.PodsOwnedBy(options.Namespace, job.Uid, cancellationToken);
            if (!SameVersions(latest, fresh) || latest.Any(pod => pod.Executed))
            {
                tally.Protected++;
                return;
            }

            // No retries by name after a conflict or a superseding attempt.
            if ((await cluster.ReadJob(options.Namespace, job.Name, cancellationToken)).Fingerprint != current.Fingerprint)
            {
                tally.Protected++;
                return;
            }

            await cluster.DeleteJob(options.Namespace, job, cancellationToken);
            tally.Deleted++;
        }
        catch (ClusterReadException error) when (error.StatusCode is not null)
        {
            tally.Protected++;
            if (error.StatusCode is 404 or 409)
            {
                tally.Failed++;
            }
            else
            {
                tally.Unknown++;
            }
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            tally.Protected++;
            tally.Unknown++;
        }
    }

    async Task<bool> Eligible(AgentJob job, IReadOnlyList<AgentPod> pods, TimeProvider time, CancellationToken cancellationToken)
    {
        if (!HasLabels(job.Labels, options.JobLabels) || job.BackoffLimit != 0 || job.Suspended || job.Deleting || job.Finished)
        {
            return false;
        }

        var now = time.GetUtcNow();
        if (now - (job.Created ?? throw new InvalidOperationException("the Job has no creation time")) < options.Grace)
        {
            return false;
        }

        if (pods.Count == 0)
        {
            throw new InvalidOperationException("missing Pod evidence");
        }

        var podLabels = options.JobLabels.Concat(options.PodLabels).ToDictionary(label => label.Key, label => label.Value, StringComparer.Ordinal);
        foreach (var pod in pods)
        {
            if (!IsUntouchedWorkerPod(job, pod, podLabels))
            {
                return false;
            }

            if (now - (pod.Created ?? throw new InvalidOperationException("the Pod has no creation time")) < options.Grace)
            {
                return false;
            }

            if (!await MissingSecretConfirmed(pod, now, cancellationToken))
            {
                throw new InvalidOperationException("missing required-secret FailedMount evidence");
            }
        }

        return true;
    }

    bool IsUntouchedWorkerPod(AgentJob job, AgentPod pod, Dictionary<string, string> podLabels) =>
        HasLabels(pod.Labels, podLabels)
        && pod.Controllers.Count == 1
        && string.Equals(pod.Controllers[0].Kind, "Job", StringComparison.Ordinal)
        && string.Equals(pod.Controllers[0].ApiVersion, "batch/v1", StringComparison.Ordinal)
        && pod.Controllers[0].Uid == job.Uid
        && pod.Controllers[0].Name == job.Name
        && !pod.Deleting
        && pod.Phase == "Pending"
        && !pod.Executed;

    async Task<bool> MissingSecretConfirmed(AgentPod pod, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var events = await cluster.PodEvents(options.Namespace, pod.Uid, cancellationToken);
        foreach (var secret in pod.RequiredSecrets)
        {
            if (await cluster.SecretExists(options.Namespace, secret, cancellationToken))
            {
                continue;
            }

            if (events.Any(evidence => IsEvidence(evidence, pod, secret, now, options.RecentEvidence)))
            {
                return true;
            }
        }

        return false;
    }

    sealed class Tally
    {
        internal int Considered { get; set; }

        internal int Protected { get; set; }

        internal int Failed { get; set; }

        internal int Deleted { get; set; }

        internal int Unknown { get; set; }

        internal int Candidates { get; set; }

        internal int Running { get; set; }

        internal int Exited { get; set; }

        internal AgentHarnessCounts ToCounts() => new()
        {
            Considered = Considered,
            Protected = Protected,
            Failed = Failed,
            Deleted = Deleted,
            Unknown = Unknown,
            Candidates = Candidates,
            Running = Running,
            Exited = Exited
        };
    }
}
