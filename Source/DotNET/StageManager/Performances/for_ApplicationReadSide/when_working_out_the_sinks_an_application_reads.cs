// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Performances.for_ApplicationReadSide;

public class when_working_out_the_sinks_an_application_reads : Specification
{
    ApplicationReadSide _readSide;

    void Because() => _readSide = ApplicationReadSide.For(new(), ["DOCUMENTS", "reporting", "warehouse", "organization"], ProductionSamples.WithSinks(), ProductionSamples.Names);

    [Fact] void should_bind_the_mongodb_sink_by_the_name_the_production_gives_it() => _readSide.MongoDBSinks.Single().Name.ShouldEqual("documents");
    [Fact] void should_bind_the_secret_key_of_the_sink() => _readSide.MongoDBSinks.Single().Connection.ShouldEqual(new ResolvedSecretRef("documents-mongo", "connectionString"));
    [Fact] void should_name_the_file_for_arc() => _readSide.EnvironmentVariables["Cratis__MongoDB__Sinks__documents__ServerFile"].ShouldEqual("/run/secrets/sinks/documents");
    [Fact] void should_name_the_sinks_arc_cannot_read() => _readSide.Unreadable.ShouldContainOnly(["reporting", "warehouse", "organization"]);
    [Fact] void should_not_read_the_graph() => _readSide.Graph.ShouldBeNull();
    [Fact] void should_put_no_secret_in_an_environment_variable() => _readSide.SecretEnvironmentVariables.Count.ShouldEqual(0);
}
#endif
