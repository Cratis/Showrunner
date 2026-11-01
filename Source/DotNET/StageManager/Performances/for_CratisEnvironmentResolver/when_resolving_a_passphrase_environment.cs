// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Environments;
using Cratis.Environments.Secrets;

namespace Cratis.Showrunner.StageManager.Performances.for_CratisEnvironmentResolver;

public class when_resolving_a_passphrase_environment : Specification
{
    const string Passphrase = "correct horse battery staple";
    const string Secret = "fake-secret-value-d41d";

    string _file;
    EnvironmentResolution _resolved;
    EnvironmentResolution _wrongPassphrase;
    EnvironmentResolution _noPassphrase;

    async Task Establish()
    {
        var root = Directory.CreateTempSubdirectory("resolver-spec-").FullName;
        var environments = ProjectEnvironments.ForProject(root);
        var environment = ProjectEnvironment.Create(environments, "production", null, PassphraseSources.Fixed(Passphrase));
        await environment.SetAsync("authentication.providers.GitHub.clientId", "plain-client-id", secret: false);
        await environment.SetAsync("authentication.providers.GitHub.clientSecret", Secret, secret: true);
        _file = await File.ReadAllTextAsync(environments.PathFor("production"));
        Directory.Delete(root, recursive: true);
    }

    async Task Because()
    {
        var resolver = new CratisEnvironmentResolver(Substitute.For<ILogger<CratisEnvironmentResolver>>());
        _resolved = await resolver.ResolveAsync("production", _file, Passphrase, CancellationToken.None);
        _wrongPassphrase = await resolver.ResolveAsync("production", _file, "not the passphrase", CancellationToken.None);
        _noPassphrase = await resolver.ResolveAsync("production", _file, null, CancellationToken.None);
    }

    [Fact] void should_not_keep_the_secret_in_the_file() => _file.Contains(Secret, StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_resolve_plain_values() => _resolved.Environment!.Values["authentication.providers.GitHub.clientId"].ShouldEqual("plain-client-id");
    [Fact] void should_decrypt_secrets() => _resolved.Environment!.Values["authentication.providers.GitHub.clientSecret"].ShouldEqual(Secret);
    [Fact] void should_fail_with_the_wrong_passphrase() => _wrongPassphrase.Environment.ShouldBeNull();
    [Fact] void should_not_reveal_a_value_when_it_fails() => _wrongPassphrase.Failure.Contains(Secret, StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_fail_without_a_passphrase() => _noPassphrase.Environment.ShouldBeNull();
    [Fact] void should_say_a_passphrase_is_needed() => _noPassphrase.Failure.Contains("passphrase", StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
}
#endif
