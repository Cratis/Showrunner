// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner;

public class when_a_database_is_not_claimed : a_cluster
{
    void Establish() => _existing.AddRange(["legacy", "Studio+jobs+acme"]);

    void Because() => Plan();

    [Fact] void should_report_it_as_unclaimed() => _plan.Unclaimed.ShouldContainOnly(["Studio+jobs+acme", "legacy"]);
    [Fact] void should_not_back_it_up() => _plan.Backups.Any(backup => backup.Database == "legacy").ShouldBeFalse();
    [Fact] void should_keep_the_databases_of_the_server_apart() => _plan.ServerOwned.ShouldContainOnly(["admin", "config", "local"]);
    [Fact] void should_not_call_the_databases_of_the_server_unclaimed() => _plan.Unclaimed.Contains("admin").ShouldBeFalse();
}
#endif
