// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Keys;

namespace Cratis.Showrunner.StageManager.Understudies.for_VaultHttpKeyReader;

public class when_reading_a_namespace : Specification
{
    StubHttpHandler _handler;
    IReadOnlyList<VaultSecret> _secrets;
    byte[] _snapshot;
    IReadOnlyList<VaultSecret> _missing;

    void Establish()
    {
        _handler = new();
        _handler.Json["/v1/secret/metadata/Studio/acme"] = """{"data":{"keys":["keys/","fences/"]}}""";
        _handler.Json["/v1/secret/metadata/Studio/acme/keys"] = """{"data":{"keys":["1","2"]}}""";
        _handler.Json["/v1/secret/metadata/Studio/acme/fences"] = """{"data":{"keys":["subject-7"]}}""";
        _handler.Json["/v1/secret/data/Studio/acme/keys/1"] = """{"data":{"data":{"key":"one"},"metadata":{}}}""";
        _handler.Json["/v1/secret/data/Studio/acme/keys/2"] = """{"data":{"data":{"key":"two","generation":2},"metadata":{}}}""";
        _handler.Json["/v1/secret/data/Studio/acme/fences/subject-7"] = """{"data":{"data":{"erased":"true"},"metadata":{}}}""";
        _handler.Binary["/v1/sys/storage/raft/snapshot"] = Encoding.UTF8.GetBytes("snapshot");
    }

    async Task Because()
    {
        using var reader = new VaultHttpKeyReader(new HttpClient(_handler) { BaseAddress = new Uri("https://vault.example:8200") }, "vault-token-value");
        _secrets = await reader.ReadTreeAsync("Studio/acme", CancellationToken.None);
        _missing = await reader.ReadTreeAsync("Studio/nobody", CancellationToken.None);
        _snapshot = await reader.TakeSnapshotAsync(CancellationToken.None);
    }

    [Fact] void should_read_every_secret_under_the_namespace_in_any_depth() => _secrets.Select(secret => secret.Path).ShouldContainOnly(["Studio/acme/keys/1", "Studio/acme/keys/2", "Studio/acme/fences/subject-7"]);
    [Fact] void should_read_the_values() => _secrets.Single(secret => secret.Path == "Studio/acme/keys/1").Data["key"].ShouldEqual("one");
    [Fact] void should_keep_a_value_that_is_not_text() => _secrets.Single(secret => secret.Path == "Studio/acme/keys/2").Data["generation"].ShouldEqual("2");
    [Fact] void should_send_the_token_as_a_header() => _handler.Requests.TrueForAll(request => request.Headers.GetValues("X-Vault-Token").Single() == "vault-token-value").ShouldBeTrue();
    [Fact] void should_never_put_the_token_in_a_url() => _handler.Requests.Exists(request => request.RequestUri!.ToString().Contains("vault-token-value", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_find_no_secrets_for_a_namespace_that_has_none() => _missing.ShouldBeEmpty();
    [Fact] void should_take_a_raft_snapshot() => Encoding.UTF8.GetString(_snapshot).ShouldEqual("snapshot");
}
#endif
