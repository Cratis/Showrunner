// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;

namespace Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob;

public class when_taking_a_vault_snapshot_into_a_versioned_bucket : a_vault_with_keys
{
    async Task Because()
    {
        _storage.Versioned = true;
        var job = new KeyBackupJob(_storage, new FakeChronicleDirectory(new([])), _vault, _publicCertificate, new FixedTimeProvider(_now), Substitute.For<ILogger<KeyBackupJob>>());
        _result = await job.SnapshotAsync(_configuration, CancellationToken.None);
    }

    [Fact] void should_refuse() => _result.Refused.ShouldBeTrue();
    [Fact] void should_not_take_the_snapshot() => _vault.Asked.ShouldBeEmpty();
}
#endif
