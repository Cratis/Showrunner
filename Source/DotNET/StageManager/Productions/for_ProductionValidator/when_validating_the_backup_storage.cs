// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionValidator;

public class when_validating_the_backup_storage : Specification
{
    IReadOnlyList<ProductionProblem> _complete;
    IReadOnlyList<ProductionProblem> _nothingNamed;
    IReadOnlyList<ProductionProblem> _agentOff;
    IReadOnlyList<ProductionProblem> _external;
    IReadOnlyList<ProductionProblem> _badTask;

    static IReadOnlyList<ProductionProblem> Validate(Action<ProductionDatabase> change)
    {
        var spec = ProductionSamples.Managed();
        spec.Database.Backup.Enabled = true;
        change(spec.Database);
        return ProductionValidator.ValidateEach(spec);
    }

    static void Storage(ProductionDatabase database, string endpoint = "objects.example.com", string secret = "backup-credentials") =>
        database.Backup.Storage = new() { Bucket = "backups", Endpoint = endpoint, CredentialsSecretName = secret };

    void Because()
    {
        _complete = Validate(database => Storage(database));
        _nothingNamed = Validate(database => Storage(database, string.Empty, string.Empty));
        _agentOff = Validate(database =>
        {
            Storage(database);
            database.Backup.Enabled = false;
        });
        _external = ProductionValidator.ValidateEach(ExternalWithStorage());
        _badTask = Validate(database =>
        {
            Storage(database);
            database.Backup.Storage.Tasks = [new() { Name = "hourly", Schedule = string.Empty, Keep = 0 }];
        });
    }

    static ProductionSpec ExternalWithStorage()
    {
        var spec = ProductionSamples.External();
        spec.Database.Backup.Storage = new() { Bucket = "backups", Endpoint = "objects.example.com", CredentialsSecretName = "backup-credentials" };
        return spec;
    }

    [Fact] void should_accept_a_complete_storage() => _complete.ShouldBeEmpty();
    [Fact] void should_require_the_endpoint() => _nothingNamed.Select(problem => problem.Message).ShouldContain("database.backup.storage.endpoint is required when a bucket is named.");
    [Fact] void should_require_the_credentials() => _nothingNamed.Select(problem => problem.Message).ShouldContain("database.backup.storage.credentialsSecretName is required when a bucket is named.");
    [Fact] void should_require_the_agent() => _agentOff.Single().Message.ShouldContain("database.backup.enabled");
    [Fact] void should_refuse_a_storage_on_an_external_database() => _external.Single().Message.ShouldContain("database.mode Managed");
    [Fact] void should_refuse_a_task_without_a_schedule() => _badTask.Single().Message.ShouldContain("cron schedule");
}
#endif
