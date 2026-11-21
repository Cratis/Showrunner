// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// What the agent harness watchdog reads of a Pod that belongs to a worker Job.
/// </summary>
/// <param name="Name">The Pod name.</param>
/// <param name="Uid">The Pod UID.</param>
/// <param name="ResourceVersion">The resource version.</param>
/// <param name="Labels">The Pod labels.</param>
/// <param name="Created">When the Pod was created; <see langword="null"/> when the API did not say.</param>
/// <param name="Phase">The Pod phase.</param>
/// <param name="Deleting">Whether the Pod is being deleted.</param>
/// <param name="Owners">The owner references.</param>
/// <param name="RequiredSecrets">The Secrets mounted as non-optional volumes.</param>
/// <param name="Executed">Whether anything shows a container ever started, or the Pod became ready.</param>
/// <param name="ExitedNonZero">Whether a container terminated with a failing exit code.</param>
public sealed record AgentPod(
    string Name,
    string Uid,
    string ResourceVersion,
    IReadOnlyDictionary<string, string> Labels,
    DateTimeOffset? Created,
    string? Phase,
    bool Deleting,
    IReadOnlyList<AgentOwner> Owners,
    IReadOnlyList<string> RequiredSecrets,
    bool Executed,
    bool ExitedNonZero)
{
    /// <summary>
    /// Gets the owner references that are controllers.
    /// </summary>
    public IReadOnlyList<AgentOwner> Controllers => [.. Owners.Where(owner => owner.Controller)];

    /// <summary>
    /// Gets the identity and version of the Pod, which changes whenever anything about it does.
    /// </summary>
    public string Fingerprint => $"{Uid}@{ResourceVersion}";
}
