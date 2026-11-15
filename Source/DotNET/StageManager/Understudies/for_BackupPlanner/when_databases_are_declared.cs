// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner;

public class when_databases_are_declared : a_cluster
{
    protected override IReadOnlyList<string> Declared => ["{eventStore}+jobs+{namespace}", "{eventStore}+audit", "orleans"];

    void Establish() => _existing.AddRange(["Studio+jobs+acme", "Studio+jobs+Default", "Billing+jobs+Default", "Studio+audit", "orleans"]);

    void Because() => Plan();

    [Fact] void should_put_a_per_namespace_database_in_its_namespace() => Claim("Studio+jobs+acme").Namespace.ShouldEqual("acme");
    [Fact] void should_put_a_per_namespace_database_in_its_event_store() => Claim("Studio+jobs+acme").EventStore.ShouldEqual("Studio");
    [Fact] void should_not_give_it_a_scope_of_its_own() => _plan.Backups.Select(backup => (backup.EventStore, backup.Namespace)).Distinct().Count().ShouldEqual(6);
    [Fact] void should_take_a_per_namespace_pattern_for_every_namespace() => Of(BackupType.Database).Count(backup => backup.Database.Contains("+jobs+", StringComparison.Ordinal)).ShouldEqual(3);
    [Fact] void should_take_a_per_event_store_pattern_once_per_store() => Claim("Studio+audit").Namespace.ShouldBeNull();
    [Fact] void should_take_a_fixed_name_once_for_the_production() => Claim("orleans").EventStore.ShouldBeNull();
    [Fact] void should_leave_nothing_unclaimed() => _plan.Unclaimed.ShouldBeEmpty();
    [Fact] void should_not_mark_a_declared_database_that_does_not_exist_as_present() => Claim("Billing+audit").Present.ShouldBeFalse();
}
#endif
