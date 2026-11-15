// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob;

/// <summary>
/// With <c>vault.mode: None</c> the keys travel inside the Events backups, so nothing separate is taken.
/// </summary>
public class when_the_production_has_no_vault : a_vault_with_keys
{
    async Task Because() => await Run(hasVault: false);

    [Fact] void should_succeed() => _result.Succeeded.ShouldBeTrue();
    [Fact] void should_take_nothing() => _result.Taken.ShouldBeEmpty();
    [Fact] void should_not_touch_vault() => _vault.Asked.ShouldBeEmpty();
    [Fact] void should_not_touch_the_bucket() => _storage.Writes.ShouldBeEmpty();
}
#endif
