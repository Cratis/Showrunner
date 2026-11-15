// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner;

/// <summary>
/// Chronicle was given a prefix the Understudy was not told: its databases are claimed by no rule, so they show up as unclaimed instead of being skipped.
/// </summary>
public class when_the_names_do_not_follow_the_prefix : a_cluster
{
    void Establish() => _existing.AddRange(["old-Studio+es", "old-Studio"]);

    void Because() => Plan();

    [Fact] void should_report_the_databases_nobody_claims() => _plan.Unclaimed.ShouldContainOnly(["old-Studio", "old-Studio+es"]);
    [Fact] void should_not_back_them_up() => _plan.Backups.Any(backup => backup.Database.StartsWith("old-", StringComparison.Ordinal)).ShouldBeFalse();
}
#endif
