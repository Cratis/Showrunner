// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_PbmBackupsCheck;

public class when_backups_are_ready_and_recent : a_check
{
    protected override IWatchdogCheck Check => new PbmBackupsCheck(_reader, "databases", TimeSpan.FromHours(3));

    void Establish()
    {
        // The newest completion decides, not list order and not creation time.
        _reader.Backups.Add(new PbmBackupInfo("old", "ready", _time.Now.AddHours(-1), _time.Now.AddHours(-30), null));
        _reader.Backups.Add(new PbmBackupInfo("new", "ready", _time.Now.AddDays(-2), _time.Now.AddMinutes(-90), null));
        _reader.Backups.Add(new PbmBackupInfo("running", "running", _time.Now.AddMinutes(-5), null, null));
    }

    async Task Because() => await Run();

    [Fact] void should_be_healthy() => _findings.ShouldBeEmpty();
}
#endif
