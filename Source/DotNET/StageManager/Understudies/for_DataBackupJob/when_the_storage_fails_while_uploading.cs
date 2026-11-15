// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_DataBackupJob.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_DataBackupJob;

public class when_the_storage_fails_while_uploading : a_seeded_production
{
    Exception? _exception;

    void Establish() => _storage.Unreachable = true;

    async Task Because() => _exception = await Catch.Exception(Run);

    [Fact] void should_not_report_success() => _result.ShouldBeNull();
    [Fact] void should_fail_the_job() => _exception.ShouldNotBeNull();
    [Fact] void should_leave_no_archive_in_the_work_directory() => Directory.GetFiles(_work).ShouldBeEmpty();
}
#endif
