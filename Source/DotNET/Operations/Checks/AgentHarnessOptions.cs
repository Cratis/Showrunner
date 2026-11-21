// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// The options of <see cref="AgentHarnessCheck"/>.
/// </summary>
public sealed record AgentHarnessOptions
{
    /// <summary>
    /// Gets the namespace of the agent workers.
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Gets the labels a Job must carry to be a worker Job.
    /// </summary>
    public IReadOnlyDictionary<string, string> JobLabels { get; init; } = new Dictionary<string, string> { ["app.kubernetes.io/managed-by"] = "cratis-ai-agents" };

    /// <summary>
    /// Gets the extra labels a worker Pod must carry.
    /// </summary>
    public IReadOnlyDictionary<string, string> PodLabels { get; init; } = new Dictionary<string, string> { ["cratis.io/agent-workload"] = "worker" };

    /// <summary>
    /// Gets a value indicating whether Jobs that are provably stuck are deleted. When false the check only reports them.
    /// </summary>
    public bool CleanupEnabled { get; init; }

    /// <summary>
    /// Gets how old a Job and its Pods must be before they are judged.
    /// </summary>
    public TimeSpan Grace { get; init; } = TimeSpan.FromSeconds(1200);

    /// <summary>
    /// Gets how recently the missing-Secret event must have been observed.
    /// </summary>
    public TimeSpan RecentEvidence { get; init; } = TimeSpan.FromSeconds(300);

    /// <summary>
    /// Gets the most Jobs one pass may delete. More candidates than this is a failure and deletes none.
    /// </summary>
    public int Cap { get; init; } = 3;

    /// <summary>
    /// Gets the wall-clock budget of one pass.
    /// </summary>
    public TimeSpan Budget { get; init; } = TimeSpan.FromSeconds(120);
}
