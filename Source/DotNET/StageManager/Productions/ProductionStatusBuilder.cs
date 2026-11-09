// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Resources;

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Collects the conditions of a reconciliation and writes them to the status of a Production. Messages are written by the reconciliation and never carry a secret value.
/// </summary>
/// <param name="production">The Production whose status is built.</param>
/// <param name="time">The time provider.</param>
public class ProductionStatusBuilder(Production production, TimeProvider time)
{
    readonly List<ResourceCondition> _conditions = [];
    readonly Dictionary<string, ResourceCondition> _previous = (production.Status?.Conditions ?? [])
        .GroupBy(condition => condition.Type, StringComparer.Ordinal)
        .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

    /// <summary>
    /// Gets the phase the Production ends up in.
    /// </summary>
    public ProductionPhase Phase { get; private set; } = ProductionPhase.Pending;

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
    /// Ends the reconciliation as blocked: nothing was applied.
    /// </summary>
    public void Block() => Phase = ProductionPhase.Blocked;

    /// <summary>
    /// Ends the reconciliation after the resources were applied.
    /// </summary>
    /// <param name="ready">Whether Chronicle, its database and its graph are ready.</param>
    public void Applied(bool ready) => Phase = ready ? ProductionPhase.Ready : ProductionPhase.Progressing;

    /// <summary>
    /// Writes the collected conditions and the phase to the status.
    /// </summary>
    public void Write()
    {
        var status = production.Status;
        status.Phase = Phase;
        status.ObservedGeneration = production.Metadata.Generation ?? 0;
        status.Conditions = [.. _conditions];
    }
}
