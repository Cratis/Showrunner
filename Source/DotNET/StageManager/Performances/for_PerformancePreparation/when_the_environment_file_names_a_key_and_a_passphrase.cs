// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_the_environment_file_names_a_key_and_a_passphrase : a_preparation
{
    void Establish()
    {
        _performance.Spec.Environment.ConfigMapRef.Key = "env.enc";
        _performance.Spec.Environment.PassphraseSecretRef = new() { Name = "crm-passphrase" };
        _cluster.ConfigMapValues["apps/crm-environment/env.enc"] = "encrypted-file";
        _cluster.SecretValues["apps/crm-passphrase/passphrase"] = "fake-passphrase";
    }

    Task Because() => Prepare();

    [Fact] void should_prepare_the_performance() => _prepared.ShouldNotBeNull();
    [Fact] void should_read_the_named_key() => _resolver.File.ShouldEqual("encrypted-file");
    [Fact] void should_read_the_passphrase_from_the_default_key() => _resolver.Passphrase.ShouldEqual("fake-passphrase");
}
#endif
