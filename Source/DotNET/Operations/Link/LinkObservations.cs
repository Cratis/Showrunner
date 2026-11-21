// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Link.Protocol;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// Turns what a pass found into the facts Direct understands: a check that fails is <c>check-failed</c> (on its first and second
/// consecutive failure - Direct raises an alert from the second - and then once an hour while it lasts), a check that passes again
/// is <c>check-recovered</c>, and a remedy whose verification check passed is <c>remedy-verified</c>.
/// </summary>
/// <param name="options">The link options.</param>
/// <param name="timeProvider">The clock.</param>
internal sealed class LinkObservations(DirectLinkOptions options, TimeProvider timeProvider)
{
    /// <summary>
    /// The prefix of the finding key of one of Direct's endpoint checks.
    /// </summary>
    internal const string EndpointPrefix = "direct-check:";

    const int RepeatEvery = 12;

    /// <summary>
    /// Records the pass.
    /// </summary>
    /// <param name="context">The pass.</param>
    /// <param name="findings">The confirmed findings by key.</param>
    /// <param name="endpoints">The results of Direct's endpoint checks that ran this pass.</param>
    internal void Record(LinkContext context, IReadOnlyDictionary<string, Finding> findings, IReadOnlyCollection<EndpointCheckResult> endpoints)
    {
        var now = timeProvider.GetUtcNow();
        var revision = context.Active(now)?.Snapshot.Revision ?? 0;
        var observations = new Dictionary<string, CheckObservation>(context.Memory.Observations, StringComparer.Ordinal);
        var failing = findings.Where(finding => !finding.Key.StartsWith(EndpointPrefix, StringComparison.Ordinal)).ToList();

        foreach (var (key, finding) in failing)
        {
            Fail(context, observations, key, options.EffectiveResource, 0, Redaction.Summarize(finding.Text), revision, now);
        }

        foreach (var key in observations.Where(entry => entry.Value.State == ObservedCheckState.Failing).Select(entry => entry.Key).ToList())
        {
            if (!failing.Exists(finding => finding.Key == key) && !endpoints.Any(result => result.Name == key) && !IsEndpointCheck(context, key))
            {
                Recover(context, observations, key, options.EffectiveResource, 0, revision, now);
                observations.Remove(key);
            }
        }

        foreach (var result in endpoints)
        {
            if (result.Passed)
            {
                Recover(context, observations, result.Name, result.Resource, result.StatusCode, revision, now);
            }
            else
            {
                Fail(context, observations, result.Name, result.Resource, result.StatusCode, result.Reason, revision, now);
            }
        }

        context.Memory = context.Memory with { Observations = observations };
        Verify(context, now);
    }

    /// <summary>
    /// Whether the held configuration, even a lapsed one, defines the check: its state only changes when it runs.
    /// </summary>
    /// <param name="context">The pass.</param>
    /// <param name="name">The check's name.</param>
    /// <returns>True when Direct defines it.</returns>
    static bool IsEndpointCheck(LinkContext context, string name) =>
        context.Identity?.Held?.Snapshot.Checks.Any(check => check.Name == name) == true;

    static void Recover(LinkContext context, Dictionary<string, CheckObservation> observations, string name, Guid resource, int statusCode, long revision, DateTimeOffset now)
    {
        if (observations.TryGetValue(name, out var previous) && previous.State == ObservedCheckState.Failing)
        {
            context.Outbox.Enqueue(new CheckRecovered(name, resource, statusCode, revision), previous.EvidenceEvent);
        }

        observations[name] = new CheckObservation(ObservedCheckState.Passing, 0, default, now, statusCode, Guid.Empty);
    }

    static void Fail(LinkContext context, Dictionary<string, CheckObservation> observations, string name, Guid resource, int statusCode, string reason, long revision, DateTimeOffset now)
    {
        var previous = observations.TryGetValue(name, out var found) && found.State == ObservedCheckState.Failing ? found : null;
        var failures = (previous?.Failures ?? 0) + 1;
        var evidence = previous?.EvidenceEvent ?? Guid.Empty;
        if (failures <= 2 || failures % RepeatEvery == 0)
        {
            evidence = context.Outbox.Enqueue(new CheckFailed(name, resource, statusCode, reason, failures, revision));
        }

        observations[name] = new CheckObservation(ObservedCheckState.Failing, failures, previous?.FirstFailedAt ?? now, now, statusCode, evidence);
    }

    static void Verify(LinkContext context, DateTimeOffset now)
    {
        var held = context.Active(now)?.Snapshot;
        foreach (var (name, _) in context.Memory.AwaitingVerification.ToList())
        {
            var remedy = held?.Remedies.FirstOrDefault(candidate => candidate.Name == name);
            if (remedy is null || !context.Memory.Observations.TryGetValue(remedy.Verification, out var observation) || observation.State != ObservedCheckState.Passing)
            {
                continue;
            }

            context.Outbox.Enqueue(new RemedyVerified(name, remedy.Resource, remedy.Verification));
            context.Memory = context.Memory with
            {
                AwaitingVerification = context.Memory.AwaitingVerification.Where(entry => entry.Key != name).ToDictionary(),
                Escalated = [.. context.Memory.Escalated.Where(escalated => escalated != name)]
            };
        }
    }
}
