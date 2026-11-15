// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner;

/// <summary>
/// Nothing in the cluster is skipped without appearing as claimed, unclaimed or owned by the database server.
/// </summary>
public class when_every_database_of_the_cluster_is_accounted_for : a_cluster
{
    protected override IReadOnlyList<string> Declared => ["{eventStore}+jobs+{namespace}", "orleans"];

    void Establish() => _existing.AddRange(["Studio+jobs+acme", "orleans", "legacy", "scratch"]);

    void Because() => Plan();

    [Fact] void should_show_each_existing_database_exactly_once()
    {
        var shown = _plan.Backups.Where(backup => backup.Present).Select(backup => backup.Database).Concat(_plan.Unclaimed).Concat(_plan.ServerOwned).ToList();
        shown.Order(StringComparer.Ordinal).ShouldEqual(_existing.Order(StringComparer.Ordinal));
    }

    [Fact] void should_claim_a_database_for_one_rule_only() => _plan.Backups.Select(backup => backup.Database).Distinct().Count().ShouldEqual(_plan.Backups.Count);
}
#endif
