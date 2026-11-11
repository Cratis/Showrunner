// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions;
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Performances.for_ApplicationReadSide;

public class when_mounting_the_named_mongodb_sinks : Specification
{
    ApplicationReadSide _readSide;

    void Because()
    {
        var production = ProductionSamples.WithSinks();
        production.Sinks.Configurations.Add(new() { Name = "archive", Kind = StorageKinds.MongoDB, ConnectionSecretRef = new() { Name = "archive-mongo", Key = "dsn" } });
        _readSide = ApplicationReadSide.For(new() { Graph = true, EventStores = ["Crm"] }, ["archive", "documents"], production, ProductionSamples.Names);
    }

    [Fact] void should_mount_from_the_sinks_volume() => _readSide.Mount!.VolumeName.ShouldEqual(ApplicationReadSide.SinksVolume);
    [Fact] void should_mount_in_the_sinks_directory() => _readSide.Mount!.MountPath.ShouldEqual(ApplicationReadSide.SinksDirectory);
    [Fact] void should_project_one_source_per_sink_in_the_order_used() => string.Join(',', _readSide.Mount!.Sources.Select(source => source.SecretName)).ShouldEqual("archive-mongo,documents-mongo");
    [Fact] void should_name_each_file_after_its_sink() => _readSide.Mount!.Sources[0].Items!["dsn"].ShouldEqual("archive");
    [Fact] void should_name_the_file_of_a_default_key_after_its_sink() => _readSide.Mount!.Sources[1].Items!["connectionString"].ShouldEqual("documents");
    [Fact] void should_need_the_sink_secrets_before_the_graph_secrets() => _readSide.Secrets.First().ShouldEqual(new ResolvedSecretRef("archive-mongo", "dsn"));
    [Fact] void should_list_the_sink_files_before_the_graph_settings() => _readSide.EnvironmentVariables.Keys.First().ShouldEqual("Cratis__MongoDB__Sinks__archive__ServerFile");
}
#endif
