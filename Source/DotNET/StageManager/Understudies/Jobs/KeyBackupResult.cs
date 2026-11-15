// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.Jobs;

/// <summary>
/// What a key backup run did.
/// </summary>
/// <param name="Taken">The manifests of the runs that were written.</param>
/// <param name="Failed">The namespaces that failed.</param>
/// <param name="BucketCheck">The verdict on the keys bucket, or null when the Production has no Vault and nothing was checked.</param>
/// <param name="Prune">What pruning did.</param>
public record KeyBackupResult(IReadOnlyList<BackupManifest> Taken, IReadOnlyList<BackupFailure> Failed, BucketRulesCheck? BucketCheck, PruneResult Prune)
{
    /// <summary>
    /// Gets whether the run was refused because the bucket rules are not in place. Nothing was read from Vault.
    /// </summary>
    public bool Refused => BucketCheck is { Holds: false };

    /// <summary>
    /// Gets whether every namespace was backed up.
    /// </summary>
    public bool Succeeded => !Refused && Failed.Count == 0;
}
