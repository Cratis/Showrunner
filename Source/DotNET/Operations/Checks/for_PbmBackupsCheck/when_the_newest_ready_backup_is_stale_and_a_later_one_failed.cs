// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_PbmBackupsCheck;

public class when_the_newest_ready_backup_is_stale_and_a_later_one_failed : a_check
{
    protected override IWatchdogCheck Check => new PbmBackupsCheck(_reader, "databases", TimeSpan.FromHours(3));

    void Establish()
    {
        _reader.Backups.Add(new PbmBackupInfo("ready-1", "ready", _time.Now.AddHours(-9), _time.Now.AddHours(-8), null));
        _reader.Backups.Add(new PbmBackupInfo("failed-old", "error", _time.Now.AddHours(-7), null, "old error"));
        _reader.Backups.Add(new PbmBackupInfo("failed-new", "error", _time.Now.AddHours(-2), null, "connect mongodb://admin:hunter2@db:27017 failed password=hunter3 " + new string('x', 500)));
    }

    async Task Because() => await Run();

    [Fact] void should_report_the_backup_as_stale() => Keys.ShouldContainOnly("pbm-backups:stale");
    [Fact] void should_explain_with_the_newest_failed_backup() => _findings.Single().Text.ShouldContain("newest failed backup failed-new");
    [Fact] void should_not_explain_with_an_older_failure() => _findings.Single().Text.ShouldNotContain("old error");
    [Fact] void should_redact_connection_strings_from_the_error() => _findings.Single().Text.ShouldNotContain("hunter2");
    [Fact] void should_redact_credentials_from_the_error() => _findings.Single().Text.ShouldNotContain("hunter3");
    [Fact] void should_truncate_the_error() => _findings.Single().Text.Length.ShouldBeLessThan(600);
}
#endif
