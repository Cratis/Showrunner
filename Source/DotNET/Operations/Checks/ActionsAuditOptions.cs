// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// The options of <see cref="ActionsScheduleAuditCheck"/>.
/// </summary>
public sealed record ActionsAuditOptions
{
    /// <summary>
    /// Gets the GitHub organization whose repositories are audited.
    /// </summary>
    public string Organization { get; init; } = "Cratis";

    /// <summary>
    /// Gets how long one audit may take. It must fit inside the per-check budget of the run.
    /// </summary>
    public TimeSpan Budget { get; init; } = TimeSpan.FromSeconds(50);

    /// <summary>
    /// Gets the most requests one audit may make.
    /// </summary>
    public int MaximumRequests { get; init; } = 400;

    /// <summary>
    /// Gets how many known scheduled workflows are remembered.
    /// </summary>
    public int MaximumRememberedSchedules { get; init; } = 2000;
}
