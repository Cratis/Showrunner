// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.Discovery;

namespace Cratis.Showrunner.StageManager.Understudies.Jobs;

/// <summary>
/// What a backup run did. A run that failed in any scope has failed: <see cref="Succeeded"/> is false and the job exits with a failure.
/// </summary>
/// <param name="Taken">The manifests of the runs that were written.</param>
/// <param name="Failed">The scopes that failed, and why.</param>
/// <param name="Prune">What pruning did.</param>
/// <param name="Plan">What the run set out to back up, including the databases no rule claims.</param>
public record BackupRunResult(IReadOnlyList<BackupManifest> Taken, IReadOnlyList<BackupFailure> Failed, PruneResult Prune, BackupPlan Plan)
{
    /// <summary>
    /// Gets whether every scope was backed up.
    /// </summary>
    public bool Succeeded => Failed.Count == 0;
}
