// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using KubernetesProvider = Pulumi.Kubernetes.Provider;

namespace Cratis.Showrunner.StageManager.Pulumi.Understudies;

/// <summary>
/// The inputs of an <see cref="Understudy"/>.
/// </summary>
public sealed class UnderstudyArgs
{
    /// <summary>
    /// Gets the Kubernetes provider the resources are created with, or null for the default one.
    /// </summary>
    public KubernetesProvider? Provider { get; init; }

    /// <summary>
    /// Gets the Kubernetes name of the Understudy, or null to use the Pulumi name of the component.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the namespace of the Understudy, which is the namespace of the Production it backs up.
    /// </summary>
    public required Input<string> Namespace { get; init; }

    /// <summary>
    /// Gets the name of the Production to back up, in the namespace of the Understudy.
    /// </summary>
    public required Input<string> ProductionName { get; init; }

    /// <summary>
    /// Gets the cron schedule of the database backups, or null for the operator's default (every hour).
    /// </summary>
    public string? Schedule { get; init; }

    /// <summary>
    /// Gets the longest a backup is kept, in days, whatever the retention tiers say, or null for the operator's default (35).
    /// </summary>
    public int? MaxAgeDays { get; init; }

    /// <summary>
    /// Gets how many backups the retention tiers keep, or null for the operator's defaults.
    /// </summary>
    public UnderstudyRetentionArgs? Retention { get; init; }

    /// <summary>
    /// Gets the prefix Chronicle was configured to give its databases, such as <c>prod-</c>.
    /// </summary>
    public string? DatabasePrefix { get; init; }

    /// <summary>
    /// Gets the databases of the Production that Chronicle does not own. A pattern with <c>{eventStore}</c> or <c>{namespace}</c>
    /// is taken per event store or namespace; a fixed name once per Production.
    /// </summary>
    public IList<string> Databases { get; init; } = [];

    /// <summary>
    /// Gets where the database backups are written.
    /// </summary>
    public required UnderstudyStorageArgs Storage { get; init; }

    /// <summary>
    /// Gets how the keys of the Production's Vault are backed up, or null for no separate key backups.
    /// </summary>
    public UnderstudyKeysArgs? Keys { get; init; }

    /// <summary>
    /// Gets the image of the job that takes the backups, or null for the Understudy image of the operator's release.
    /// </summary>
    public string? Image { get; init; }

    /// <summary>
    /// Gets the size of the scratch space a backup job dumps into before it uploads, such as <c>10Gi</c>. It must hold the largest database.
    /// </summary>
    public string? WorkSize { get; init; }

    /// <summary>
    /// Gets a value indicating whether the scheduled backups are paused.
    /// </summary>
    public bool Suspend { get; init; }

    /// <summary>
    /// Gets the labels put on the Understudy and on the Secrets the component writes.
    /// </summary>
    public IDictionary<string, string> Labels { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the resources the Understudy and its Secrets wait for, such as the Production.
    /// </summary>
    public IList<Resource> DependsOn { get; init; } = [];
}
