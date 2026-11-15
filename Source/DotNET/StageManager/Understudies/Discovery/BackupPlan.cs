// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Discovery;

/// <summary>
/// What an Understudy backs up. Every database in the cluster appears exactly once: in <see cref="Backups"/> when a rule claims it,
/// in <see cref="Unclaimed"/> when none does, or in <see cref="ServerOwned"/> when the database server owns it.
/// </summary>
/// <param name="Backups">The databases the rules claim.</param>
/// <param name="Keys">The namespaces whose keys are backed up, when the Production has a Vault.</param>
/// <param name="Unclaimed">The databases no rule claims. They are reported in the status and the API, and not backed up.</param>
/// <param name="ServerOwned">The databases the database server owns (<c>admin</c>, <c>local</c> and <c>config</c>), which a disaster recovery snapshot covers.</param>
public record BackupPlan(
    IReadOnlyList<PlannedBackup> Backups,
    IReadOnlyList<PlannedKeys> Keys,
    IReadOnlyList<string> Unclaimed,
    IReadOnlyList<string> ServerOwned);
