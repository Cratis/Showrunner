// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.Discovery;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner.given;

/// <summary>
/// A Chronicle with two event stores, Studio in its default namespace and in acme, and Billing in its default namespace, on a database server that holds their databases.
/// </summary>
public abstract class a_cluster : Specification
{
    protected ChronicleTopology _topology;
    protected List<string> _existing;
    protected BackupPlan _plan;

    protected virtual string Prefix => string.Empty;

    protected virtual IReadOnlyList<string> Declared => [];

    protected virtual bool HasVault => false;

    protected IEnumerable<PlannedBackup> Of(BackupType type) => _plan.Backups.Where(backup => backup.Type == type);

    protected PlannedBackup Claim(string database) => _plan.Backups.Single(backup => backup.Database == database);

    void Establish()
    {
        _topology = new([new("Studio", ["Default", "acme"]), new("Billing", ["Default"])]);
        _existing =
        [
            Prefix + "chronicle+main",
            Prefix + "Studio+es",
            Prefix + "Studio+es+Default",
            Prefix + "Studio+es+acme",
            Prefix + "Studio",
            Prefix + "Studio+acme",
            Prefix + "Billing+es",
            Prefix + "Billing+es+Default",
            Prefix + "Billing",
            "admin",
            "local",
            "config"
        ];
    }

    protected void Plan() => _plan = BackupPlanner.Plan(_topology, _existing, new(new ChronicleDatabaseNames(Prefix), Declared, HasVault));
}
#endif
