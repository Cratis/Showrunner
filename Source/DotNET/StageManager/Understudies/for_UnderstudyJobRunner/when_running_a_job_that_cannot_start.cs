// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyJobRunner;

/// <summary>
/// A job that cannot do its work exits with a failure, so the Kubernetes Job fails and the operator reports it. It never ends in success.
/// </summary>
public class when_running_a_job_that_cannot_start : Specification
{
    int _unsupported;
    int _noConfiguration;
    int _noChronicle;
    int _noMongoDB;

    Dictionary<string, string> Environment(bool chronicle, bool mongo)
    {
        var environment = new Dictionary<string, string>
        {
            [UnderstudyJobEnvironment.Configuration] = UnderstudyWorkload.Configuration(UnderstudySamples.Understudy(UnderstudySamples.Spec()), UnderstudySamples.Production(), UnderstudyJobKinds.Backup).ToJson()
        };
        if (chronicle)
        {
            environment[UnderstudyJobEnvironment.ChronicleConnection] = "chronicle://localhost:35000";
        }

        if (mongo)
        {
            environment[UnderstudyJobEnvironment.MongoDBConnection] = "mongodb://localhost:1";
        }

        return environment;
    }

    async Task Because()
    {
        _unsupported = await UnderstudyJobRunner.RunAsync("defragment", Environment(true, true), NullLoggerFactory.Instance, CancellationToken.None, new FakeObjectStorageFactory());
        _noConfiguration = await UnderstudyJobRunner.RunAsync(UnderstudyJobKinds.Backup, new Dictionary<string, string>(), NullLoggerFactory.Instance, CancellationToken.None, new FakeObjectStorageFactory());
        _noChronicle = await UnderstudyJobRunner.RunAsync(UnderstudyJobKinds.Backup, Environment(false, true), NullLoggerFactory.Instance, CancellationToken.None, new FakeObjectStorageFactory());
        _noMongoDB = await UnderstudyJobRunner.RunAsync(UnderstudyJobKinds.Backup, Environment(true, false), NullLoggerFactory.Instance, CancellationToken.None, new FakeObjectStorageFactory());
    }

    [Fact] void should_fail_for_a_kind_it_does_not_know() => _unsupported.ShouldEqual(UnderstudyJobRunner.Failed);
    [Fact] void should_fail_without_a_configuration() => _noConfiguration.ShouldEqual(UnderstudyJobRunner.Failed);
    [Fact] void should_fail_without_a_chronicle_connection() => _noChronicle.ShouldEqual(UnderstudyJobRunner.Failed);
    [Fact] void should_fail_without_a_database_connection() => _noMongoDB.ShouldEqual(UnderstudyJobRunner.Failed);
    [Fact] void should_never_succeed_with_nothing_done() => new[] { _unsupported, _noConfiguration, _noChronicle, _noMongoDB }.ShouldNotContain(UnderstudyJobRunner.Succeeded);
}
#endif
