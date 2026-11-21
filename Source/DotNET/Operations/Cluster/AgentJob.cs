// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// What the agent harness watchdog reads of a worker Job.
/// </summary>
/// <param name="Name">The Job name.</param>
/// <param name="Uid">The Job UID, which fences every action against a replacement of the same name.</param>
/// <param name="ResourceVersion">The resource version, which fences every action against a change.</param>
/// <param name="Labels">The Job labels.</param>
/// <param name="BackoffLimit">The back-off limit; a worker Job must have exactly 0.</param>
/// <param name="Suspended">Whether the Job is suspended.</param>
/// <param name="Deleting">Whether the Job is already being deleted.</param>
/// <param name="Finished">Whether the Job has succeeded, failed or carries a terminal condition.</param>
/// <param name="Created">When the Job was created; <see langword="null"/> when the API did not say.</param>
public sealed record AgentJob(
    string Name,
    string Uid,
    string ResourceVersion,
    IReadOnlyDictionary<string, string> Labels,
    int? BackoffLimit,
    bool Suspended,
    bool Deleting,
    bool Finished,
    DateTimeOffset? Created)
{
    /// <summary>
    /// Gets the identity and version of the Job, which changes whenever anything about it does.
    /// </summary>
    public string Fingerprint => $"{Uid}@{ResourceVersion}";
}
