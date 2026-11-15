// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.StageManager.Understudies.Keys;

namespace Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob;

public class when_vault_cannot_be_read : a_vault_with_keys
{
    sealed class FailingForGlobex(FakeVaultKeyReader inner) : IVaultKeyReader
    {
        public Task<IReadOnlyList<VaultSecret>> ReadTreeAsync(string path, CancellationToken cancellationToken) =>
            path.EndsWith("globex", StringComparison.Ordinal) ? throw new HttpRequestException($"403 for {GlobexKey}") : inner.ReadTreeAsync(path, cancellationToken);

        public Task<byte[]> TakeSnapshotAsync(CancellationToken cancellationToken) => inner.TakeSnapshotAsync(cancellationToken);
    }

    async Task Because()
    {
        var job = new KeyBackupJob(_storage, new FakeChronicleDirectory(new([new("Studio", ["acme", "globex"])])), new FailingForGlobex(_vault), _publicCertificate, new FixedTimeProvider(_now), Substitute.For<ILogger<KeyBackupJob>>());
        _result = await job.RunAsync(_configuration, CancellationToken.None);
    }

    [Fact] void should_fail_the_run() => _result.Succeeded.ShouldBeFalse();
    [Fact] void should_still_back_up_the_other_namespace() => _result.Taken.Select(manifest => manifest.Namespace).ShouldContainOnly(["acme"]);
    [Fact] void should_keep_nothing_of_the_failed_namespace() => _storage.Keys.Any(key => key.Contains("/globex/", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_not_echo_what_vault_answered() => _result.Failed.Single().Reason.Contains(GlobexKey, StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_not_say_the_run_was_refused() => _result.Refused.ShouldBeFalse();
    [Fact] void should_not_keep_a_plaintext_key() => _storage.Keys.Any(key => Encoding.UTF8.GetString(_storage.ContentOf(key)).Contains(AcmeKey, StringComparison.Ordinal)).ShouldBeFalse();
}
#endif
