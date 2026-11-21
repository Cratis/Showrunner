// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_PbmBackupsCheck;

public class when_there_is_no_ready_backup : a_check
{
    protected override IWatchdogCheck Check => new PbmBackupsCheck(_reader, "databases", TimeSpan.FromHours(3));

    void Establish() => _reader.Backups.Add(new PbmBackupInfo("running", "running", _time.Now.AddMinutes(-5), null, null));

    async Task Because() => await Run();

    [Fact] void should_report_the_backup_as_missing() => Keys.ShouldContainOnly("pbm-backups:missing");
    [Fact] void should_say_so() => _findings.Single().Text.ShouldContain("PBM has no ready backup;");
}
#endif
