// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner;

public class when_planning_default_and_named_namespaces : a_cluster
{
    void Because() => Plan();

    [Fact] void should_back_up_the_cluster_database_for_the_production() => Of(BackupType.Cluster).Select(backup => backup.Database).ShouldContainOnly(["chronicle+main"]);
    [Fact] void should_back_up_each_event_store_database() => Of(BackupType.EventStore).Select(backup => (backup.EventStore, backup.Database)).ShouldContainOnly([("Studio", "Studio+es"), ("Billing", "Billing+es")]);
    [Fact] void should_back_up_the_events_of_every_namespace() => Of(BackupType.Events).Select(backup => backup.Database).ShouldContainOnly(["Studio+es+Default", "Studio+es+acme", "Billing+es+Default"]);
    [Fact] void should_back_up_the_read_models_of_the_default_namespace_by_the_store_name() => Claim("Studio").Namespace.ShouldEqual("Default");
    [Fact] void should_back_up_the_read_models_of_a_named_namespace() => Claim("Studio+acme").Namespace.ShouldEqual("acme");
    [Fact] void should_back_up_every_read_models_database() => Of(BackupType.ReadModels).Select(backup => backup.Database).ShouldContainOnly(["Studio", "Studio+acme", "Billing"]);
    [Fact] void should_mark_every_existing_database_as_present() => _plan.Backups.All(backup => backup.Present).ShouldBeTrue();
    [Fact] void should_leave_nothing_unclaimed() => _plan.Unclaimed.ShouldBeEmpty();
    [Fact] void should_not_plan_keys_without_a_vault() => _plan.Keys.ShouldBeEmpty();
}
#endif
