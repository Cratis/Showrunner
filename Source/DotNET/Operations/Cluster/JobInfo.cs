// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// A Job.
/// </summary>
/// <param name="Name">The name.</param>
/// <param name="Created">When it was created.</param>
/// <param name="OwnerCronJobs">The names of the CronJobs that own it.</param>
/// <param name="Started">When it started.</param>
/// <param name="Complete">Whether it completed.</param>
/// <param name="Failed">Whether it failed.</param>
/// <param name="FailureReason">The reason of its Failed condition.</param>
/// <param name="FailureMessage">The message of its Failed condition.</param>
public sealed record JobInfo(
    string Name,
    DateTimeOffset? Created,
    IReadOnlyList<string> OwnerCronJobs,
    DateTimeOffset? Started = null,
    bool Complete = false,
    bool Failed = false,
    string? FailureReason = null,
    string? FailureMessage = null);
