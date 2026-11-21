// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// What the runner watchdog reads of an ephemeral runner of a GitHub Actions runner scale set.
/// </summary>
/// <param name="Name">The runner name.</param>
/// <param name="ScaleSet">The scale set it belongs to, from the <c>app.kubernetes.io/instance</c> label.</param>
/// <param name="Created">When the runner was created.</param>
/// <param name="Phase">Its phase, for example <c>Pending</c> or <c>Running</c>.</param>
/// <param name="HasJob">Whether a job is assigned to it.</param>
/// <param name="Repository">The repository of the assigned job, if known.</param>
/// <param name="JobName">The display name of the assigned job, if known.</param>
public sealed record RunnerInfo(string Name, string ScaleSet, DateTimeOffset Created, string? Phase, bool HasJob, string? Repository = null, string? JobName = null);
