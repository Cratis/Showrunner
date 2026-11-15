// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;

namespace Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob;

public class when_taking_a_vault_snapshot : a_vault_with_keys
{
    async Task Because()
    {
        var job = new KeyBackupJob(_storage, new FakeChronicleDirectory(new([])), _vault, _publicCertificate, new FixedTimeProvider(_now), Substitute.For<ILogger<KeyBackupJob>>());
        _result = await job.SnapshotAsync(_configuration, CancellationToken.None);
    }

    [Fact] void should_succeed() => _result.Succeeded.ShouldBeTrue();
    [Fact] void should_store_it_for_the_production() => _storage.Has(BackupLayout.RunPrefix("ledger", null, null, _now) + KeyBackupJob.SnapshotFile).ShouldBeTrue();
    [Fact] void should_encrypt_it_to_the_customer() => Decrypt(_storage.ContentOf(BackupLayout.RunPrefix("ledger", null, null, _now) + KeyBackupJob.SnapshotFile)).ShouldEqual("raft snapshot");
    [Fact] void should_not_store_it_in_plaintext() => Encoding.UTF8.GetString(_storage.ContentOf(BackupLayout.RunPrefix("ledger", null, null, _now) + KeyBackupJob.SnapshotFile)).Contains("raft snapshot", StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_list_it_in_the_manifest() => _result.Taken.Single().Entries.Single().Type.ShouldEqual(BackupType.VaultSnapshot);
}
#endif
