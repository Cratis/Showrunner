// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Resources;

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// The observed state of a <see cref="Understudy"/>.
/// </summary>
public class UnderstudyStatus
{
    /// <summary>
    /// Gets or sets where the Understudy is.
    /// </summary>
    [Description("Pending, Blocked, Scheduled, Healthy or Degraded.")]
    public UnderstudyPhase Phase { get; set; } = UnderstudyPhase.Pending;

    /// <summary>
    /// Gets or sets the generation of the spec the status describes.
    /// </summary>
    [Description("The generation of the spec the status describes.")]
    public long ObservedGeneration { get; set; }

    /// <summary>
    /// Gets or sets the latest backup of each type.
    /// </summary>
    [Description("The latest backup of each type, from the manifests in the bucket.")]
    public IList<LastBackupStatus> LastBackups { get; set; } = [];

    /// <summary>
    /// Gets or sets the databases no rule claims.
    /// </summary>
    [Description("Databases on the Production's database server that no rule claims. They are not backed up. Declare them in databases, or accept that they are not.")]
    public IList<string> Unclaimed { get; set; } = [];

    /// <summary>
    /// Gets or sets the backup jobs whose outcome has been reported already.
    /// </summary>
    [Description("The backup jobs whose outcome was reported already, so a restart of the operator does not report them again.")]
    public IList<string> ReportedJobs { get; set; } = [];

    /// <summary>
    /// Gets or sets the value of the Vault snapshot request that was acted on.
    /// </summary>
    [Description("The value of the showrunner.cratis.io/vault-snapshot annotation that was last acted on.")]
    public string LastVaultSnapshotRequest { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conditions that describe the resource.
    /// </summary>
    [Description("The conditions that describe the resource.")]
    public IList<ResourceCondition> Conditions { get; set; } = [];
}
