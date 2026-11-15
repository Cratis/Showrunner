// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob.given;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob;

/// <summary>
/// The run refuses to start when the bucket rules are not in place, before any key is read.
/// </summary>
public class when_the_keys_bucket_is_versioned : a_vault_with_keys
{
    void Establish() => _storage.Versioned = true;

    async Task Because() => await Run();

    [Fact] void should_refuse_the_run() => _result.Refused.ShouldBeTrue();
    [Fact] void should_not_succeed() => _result.Succeeded.ShouldBeFalse();
    [Fact] void should_say_the_bucket_is_versioned() => _result.BucketCheck!.Reason.ShouldEqual(BucketRulesCheck.Versioned);
    [Fact] void should_not_read_a_single_key() => _vault.Asked.ShouldBeEmpty();
    [Fact] void should_not_write_anything() => _storage.Writes.ShouldBeEmpty();
}
#endif
