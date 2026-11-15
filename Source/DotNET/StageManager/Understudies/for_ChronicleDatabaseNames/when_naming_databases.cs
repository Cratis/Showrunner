// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Discovery;

namespace Cratis.Showrunner.StageManager.Understudies.for_ChronicleDatabaseNames;

public class when_naming_databases : Specification
{
    ChronicleDatabaseNames _names;
    ChronicleDatabaseNames _prefixed;

    void Establish()
    {
        _names = new();
        _prefixed = new("prod-");
    }

    [Fact] void should_name_the_cluster_database() => _names.Cluster().ShouldEqual("chronicle+main");
    [Fact] void should_name_the_event_store_database() => _names.EventStore("Studio").ShouldEqual("Studio+es");
    [Fact] void should_name_the_events_of_a_namespace() => _names.Events("Studio", "acme").ShouldEqual("Studio+es+acme");
    [Fact] void should_name_the_read_models_of_the_default_namespace_after_the_store() => _names.ReadModels("Studio", "Default").ShouldEqual("Studio");
    [Fact] void should_name_the_read_models_of_another_namespace_after_store_and_namespace() => _names.ReadModels("Studio", "acme").ShouldEqual("Studio+acme");
    [Fact] void should_name_the_system_event_store_after_the_system_database() => _names.EventStore("System").ShouldEqual("_system+es");
    [Fact] void should_follow_a_configured_system_database_name() => new ChronicleDatabaseNames(SystemDatabaseName: "sys").Events("System", "Default").ShouldEqual("sys+es+Default");
    [Fact] void should_prefix_the_cluster_database() => _prefixed.Cluster().ShouldEqual("prod-chronicle+main");
    [Fact] void should_prefix_the_event_store_database() => _prefixed.EventStore("Studio").ShouldEqual("prod-Studio+es");
    [Fact] void should_prefix_the_events_database() => _prefixed.Events("Studio", "acme").ShouldEqual("prod-Studio+es+acme");
    [Fact] void should_prefix_the_read_models_database() => _prefixed.ReadModels("Studio", "Default").ShouldEqual("prod-Studio");
}
#endif
