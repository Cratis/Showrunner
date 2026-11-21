// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_PbmBackupsCheck;

public class when_the_completion_is_unreadable_or_the_newest_is_stale : a_check
{
    IReadOnlyCollection<Alerts.Finding> _unreadable;
    IReadOnlyCollection<Alerts.Finding> _newestIsStale;
    IReadOnlyCollection<Alerts.Finding> _none;
    IReadOnlyCollection<Alerts.Finding> _creationIsOldButCompletionRecent;

    protected override IWatchdogCheck Check => new PbmBackupsCheck(_reader, "databases", TimeSpan.FromHours(3));

    async Task Because()
    {
        _reader.Backups.Add(new PbmBackupInfo("unreadable", "ready", _time.Now.AddMinutes(-30), null, null));
        await Run();
        _unreadable = _findings;

        _reader.Backups.Clear();
        _reader.Backups.Add(new PbmBackupInfo("old", "ready", _time.Now.AddMinutes(-300), _time.Now.AddMinutes(-300), null));
        _reader.Backups.Add(new PbmBackupInfo("newest", "ready", _time.Now.AddMinutes(-181), _time.Now.AddMinutes(-181), null));
        await Run();
        _newestIsStale = _findings;

        _reader.Backups.Clear();
        await Run();
        _none = _findings;

        _reader.Backups.Add(new PbmBackupInfo("recent", "ready", _time.Now.AddMinutes(-300), _time.Now.AddMinutes(-30), null));
        _reader.Backups.Add(new PbmBackupInfo("old", "ready", _time.Now.AddMinutes(-300), _time.Now.AddMinutes(-300), null));
        await Run();
        _creationIsOldButCompletionRecent = _findings;
    }

    [Fact] void should_not_treat_a_ready_backup_with_an_unreadable_completion_as_healthy() => _unreadable.Select(finding => finding.Key).ShouldContainOnly("pbm-backups:api");
    [Fact] void should_use_the_newest_completion_for_the_age() => _newestIsStale.Select(finding => finding.Key).ShouldContainOnly("pbm-backups:stale");
    [Fact] void should_report_no_backups_at_all_as_missing() => _none.Select(finding => finding.Key).ShouldContainOnly("pbm-backups:missing");
    [Fact] void should_use_the_completion_and_not_the_creation_time() => _creationIsOldButCompletionRecent.ShouldBeEmpty();
}
#endif
