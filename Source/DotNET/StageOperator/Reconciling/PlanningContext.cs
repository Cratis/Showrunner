// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageOperator.Building;
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.Reconciling;

/// <summary>Represents the facts one reconcile decides from.</summary>
/// <param name="Application">The application.</param>
/// <param name="Observed">What the cluster holds for it.</param>
/// <param name="Now">The current time.</param>
/// <param name="Options">The operator configuration.</param>
public record PlanningContext(StageApplication Application, ObservedState Observed, DateTimeOffset Now, StageOperatorOptions Options)
{
    /// <summary>Gets the status the resource carries, or an empty one.</summary>
    public StageApplicationStatus Previous => Application.Status ?? new StageApplicationStatus();

    /// <summary>Gets the spec.</summary>
    public StageApplicationSpec Spec => Application.Spec;

    /// <summary>Gets the time the lifetime ends, or null when permanent.</summary>
    public DateTimeOffset? Until => Spec.Lifetime?.Permanent == true ? null : Rfc3339.Parse(Spec.Lifetime?.Until);

    /// <summary>Gets a value indicating whether the lifetime has ended.</summary>
    public bool IsExpired => Until is { } until && until <= Now;

    /// <summary>Gets the public URL, or null while the name is not valid.</summary>
    public string? Url => string.IsNullOrEmpty(Spec.PublishedName) ? null : $"https://{ApplicationNames.Host(Application, Options.BaseDomain)}";

    /// <summary>Gets the conditions builder.</summary>
    /// <returns>A builder seeded from the previous conditions.</returns>
    public StatusConditions Conditions() => new(Previous.Conditions, Now);

    /// <summary>Gets the status carried forward: everything that is not decided by the reconcile stays as it was.</summary>
    /// <returns>The base status with the generation, URL and expiry refreshed.</returns>
    public StageApplicationStatus Carry() => Previous with
    {
        ObservedGeneration = Application.Metadata?.Generation ?? 0,
        Url = Url,
        ExpiresAt = Until is { } until ? Rfc3339.Format(until) : string.Empty
    };
}
