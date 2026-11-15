// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_DataBackupJob.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_DataBackupJob;

/// <summary>
/// One database of a namespace cannot be dumped: the run fails, and nothing of that namespace is kept. The other scopes are still backed up.
/// </summary>
public class when_a_dump_fails : a_seeded_production
{
    void Establish() => _dump.Failing.Add("Studio+acme");

    async Task Because() => await Run();

    [Fact] void should_fail_the_run() => _result.Succeeded.ShouldBeFalse();
    [Fact] void should_say_which_scope_failed() => _result.Failed.Select(failure => (failure.EventStore, failure.Namespace)).ShouldContainOnly([("Studio", "acme")]);
    [Fact] void should_say_why() => _result.Failed[0].Reason.ShouldContain("Studio+acme");
    [Fact] void should_not_write_a_manifest_for_it() => _storage.Has(Prefix("Studio", "acme") + "manifest.json").ShouldBeFalse();
    [Fact] void should_not_keep_the_files_that_were_written_before_it_failed() => Files("Studio", "acme").ShouldBeEmpty();
    [Fact] void should_delete_what_it_had_written() => _storage.Deletes.ShouldContain(Prefix("Studio", "acme") + "Events.archive.gz");
    [Fact] void should_still_back_up_the_other_scopes() => _result.Taken.Count.ShouldEqual(5);
    [Fact] void should_keep_the_manifest_of_another_scope() => _storage.Has(Prefix("Billing", "Default") + "manifest.json").ShouldBeTrue();
}
#endif
