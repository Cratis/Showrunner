// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob;

public class when_the_keys_are_in_the_current_layout : a_vault_with_keys
{
    const string CurrentKey = "current-key-material-77de";

    void Establish() => _vault.Secrets["k1/Studio/acme"] = [new() { Path = "k1/Studio/acme/Studio/acme/1", Data = new Dictionary<string, string> { ["key"] = CurrentKey } }];

    async Task Because() => await Run();

    [Fact] void should_succeed() => _result.Succeeded.ShouldBeTrue();
    [Fact] void should_contain_the_keys_under_the_k1_root() => DecryptedKeys("Studio", "acme").ShouldContain(CurrentKey);
    [Fact] void should_still_contain_the_keys_of_the_earlier_layout() => DecryptedKeys("Studio", "acme").ShouldContain(AcmeKey);
}
#endif
