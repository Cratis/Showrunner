// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob;

public class when_backing_up_the_keys : a_vault_with_keys
{
    async Task Because() => await Run();

    [Fact] void should_succeed() => _result.Succeeded.ShouldBeTrue();
    [Fact] void should_apply_the_expiry_rule_to_the_bucket_before_touching_a_key() => _storage.ExpiryDays.ShouldEqual(35);
    [Fact] void should_write_one_run_per_namespace() => _result.Taken.Select(manifest => manifest.Namespace).ShouldContainOnly(["acme", "globex"]);
    [Fact] void should_decrypt_with_the_private_key() => DecryptedKeys("Studio", "acme").ShouldContain(AcmeKey);
    [Fact] void should_contain_the_fences_of_the_namespace() => DecryptedKeys("Studio", "acme").ShouldContain("Studio/acme/fences/subject-7");
    [Fact] void should_contain_the_keys_of_exactly_one_namespace() => DecryptedKeys("Studio", "acme").Contains(GlobexKey, StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_not_mix_the_other_namespace_in() => DecryptedKeys("Studio", "globex").Contains(AcmeKey, StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_not_write_a_plaintext_key_anywhere() => _storage.Keys.Any(key => Encoding.UTF8.GetString(_storage.ContentOf(key)).Contains(AcmeKey, StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_not_write_a_plaintext_fence_anywhere() => _storage.Keys.Any(key => Encoding.UTF8.GetString(_storage.ContentOf(key)).Contains("subject-7", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_list_the_keys_in_the_manifest() => _result.Taken[0].Entries.Select(entry => entry.Type).ShouldContainOnly([BackupType.Keys]);
    [Fact] void should_write_the_manifest_after_the_keys() => _storage.Writes.IndexOf(Prefix("Studio", "acme") + "manifest.json").ShouldBeGreaterThan(_storage.Writes.IndexOf(Prefix("Studio", "acme") + "Keys.cms"));
    [Fact] void should_ask_vault_for_one_namespace_at_a_time() => _vault.Asked.ShouldContainOnly(["k1/Studio/acme", "Studio/acme", "k1/Studio/globex", "Studio/globex"]);
}
#endif
