// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.Reconciling;

/// <summary>Builds the condition list of a status, keeping the transition time of a condition that did not change.</summary>
public class StatusConditions
{
    readonly IReadOnlyList<StageApplicationCondition> _previous;
    readonly DateTimeOffset _now;
    readonly Dictionary<string, StageApplicationCondition> _conditions = [];

    /// <summary>Initializes a new instance of the <see cref="StatusConditions"/> class.</summary>
    /// <param name="previous">The conditions of the previous status.</param>
    /// <param name="now">The current time.</param>
    public StatusConditions(IReadOnlyList<StageApplicationCondition> previous, DateTimeOffset now)
    {
        _previous = previous;
        _now = now;
        foreach (var type in new[] { ConditionTypes.Ready, ConditionTypes.Progressing, ConditionTypes.Degraded, ConditionTypes.Expired })
        {
            Set(type, false, "NotApplicable", string.Empty);
        }
    }

    /// <summary>Sets a condition.</summary>
    /// <param name="type">The condition type.</param>
    /// <param name="holds">Whether the condition holds.</param>
    /// <param name="reason">The machine readable reason.</param>
    /// <param name="message">The human readable message.</param>
    /// <returns>This instance, for chaining.</returns>
    public StatusConditions Set(string type, bool holds, string reason, string message)
    {
        var value = holds ? "True" : "False";
        var existing = _previous.FirstOrDefault(c => c.Type == type);
        var transition = existing is not null && existing.Status == value ? existing.LastTransitionTime : Rfc3339.Format(_now);
        _conditions[type] = new StageApplicationCondition
        {
            Type = type,
            Status = value,
            Reason = reason,
            Message = message,
            LastTransitionTime = transition
        };
        return this;
    }

    /// <summary>Builds the list in a stable order.</summary>
    /// <returns>The conditions.</returns>
    public IReadOnlyList<StageApplicationCondition> Build() => [.. _conditions.Values.OrderBy(c => c.Type, StringComparer.Ordinal)];
}
