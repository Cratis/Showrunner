// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Resources;

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// Collects the conditions of a reconciliation and writes them to the status of an Understudy. Messages never carry a secret value.
/// </summary>
/// <param name="understudy">The Understudy whose status is built.</param>
/// <param name="time">The time provider.</param>
public class UnderstudyStatusBuilder(Understudy understudy, TimeProvider time)
{
    readonly List<ResourceCondition> _conditions = [];
    readonly Dictionary<string, ResourceCondition> _previous = (understudy.Status?.Conditions ?? [])
        .GroupBy(condition => condition.Type, StringComparer.Ordinal)
        .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

    /// <summary>
    /// Gets the phase the Understudy ends up in.
    /// </summary>
    public UnderstudyPhase Phase { get; private set; } = UnderstudyPhase.Pending;

    /// <summary>
    /// Gets the conditions recorded so far.
    /// </summary>
    public IReadOnlyList<ResourceCondition> Conditions => _conditions;

    /// <summary>
    /// Records a condition. A later record of the same type replaces an earlier one.
    /// </summary>
    /// <param name="type">What the condition is about.</param>
    /// <param name="holds">Whether it holds.</param>
    /// <param name="reason">A short reason in CamelCase.</param>
    /// <param name="message">A message without any secret value.</param>
    public void Set(string type, bool holds, string reason, string message)
    {
        var status = holds ? "True" : "False";
        var changed = !_previous.TryGetValue(type, out var previous) || !string.Equals(previous.Status, status, StringComparison.Ordinal);
        _conditions.RemoveAll(condition => condition.Type == type);
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
    /// Ends the reconciliation in a phase.
    /// </summary>
    /// <param name="phase">The phase.</param>
    public void End(UnderstudyPhase phase) => Phase = phase;

    /// <summary>
    /// Writes the collected conditions and the phase to the status.
    /// </summary>
    public void Write()
    {
        var status = understudy.Status;
        status.Phase = Phase;
        status.ObservedGeneration = understudy.Metadata.Generation ?? 0;
        status.Conditions = [.. _conditions];
    }
}
