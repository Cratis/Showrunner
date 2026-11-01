// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Resources;

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Collects the conditions of a reconciliation and writes them to the status of a Performance. Messages are written by the reconciliation and never carry a secret value.
/// </summary>
/// <param name="performance">The Performance whose status is built.</param>
/// <param name="time">The time provider.</param>
public class PerformanceStatusBuilder(Performance performance, TimeProvider time)
{
    readonly List<ResourceCondition> _conditions = [];
    readonly Dictionary<string, ResourceCondition> _previous = (performance.Status?.Conditions ?? [])
        .GroupBy(condition => condition.Type, StringComparer.Ordinal)
        .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

    /// <summary>
    /// Gets the phase the Performance ends up in.
    /// </summary>
    public PerformancePhase Phase { get; private set; } = PerformancePhase.Pending;

    /// <summary>
    /// Records a condition.
    /// </summary>
    /// <param name="type">What the condition is about.</param>
    /// <param name="holds">Whether it holds.</param>
    /// <param name="reason">A short reason in CamelCase.</param>
    /// <param name="message">A message without any secret value.</param>
    public void Set(string type, bool holds, string reason, string message)
    {
        var status = holds ? "True" : "False";
        var changed = !_previous.TryGetValue(type, out var previous) || !string.Equals(previous.Status, status, StringComparison.Ordinal);
        _conditions.Add(new()
        {
            Type = type,
            Status = status,
            Reason = reason,
            Message = message,
            LastTransitionTime = changed ? time.GetUtcNow() : previous?.LastTransitionTime
        });
    }

    /// <summary>
    /// Ends the reconciliation as blocked: nothing was rolled out.
    /// </summary>
    /// <param name="missingKeys">The environment keys that are missing, when that is why.</param>
    public void Block(IEnumerable<string>? missingKeys = null)
    {
        Phase = PerformancePhase.Blocked;
        performance.Status.MissingEnvironmentKeys = [.. missingKeys ?? []];
    }

    /// <summary>
    /// Ends the reconciliation after the workloads were applied.
    /// </summary>
    /// <param name="ready">Whether every enabled component is ready.</param>
    public void Applied(bool ready)
    {
        Phase = ready ? PerformancePhase.Ready : PerformancePhase.Progressing;
        performance.Status.MissingEnvironmentKeys = [];
    }

    /// <summary>
    /// Writes the collected conditions and the phase to the status.
    /// </summary>
    public void Write()
    {
        var status = performance.Status;
        status.Phase = Phase;
        status.ObservedGeneration = performance.Metadata.Generation ?? 0;
        status.PublicUrl = PublicEndpoint.AddressOf(performance.Spec);
        status.Conditions = [.. _conditions];
    }
}
