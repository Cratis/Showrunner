// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyJobConfiguration;

public class when_round_tripping_the_configuration : Specification
{
    UnderstudyJobConfiguration _configuration;
    UnderstudyJobConfiguration _read;
    string _json;

    void Establish()
    {
        var spec = UnderstudySamples.Spec();
        spec.Keys = UnderstudySamples.Keys();
        spec.Retention.Monthly = 6;
        _configuration = UnderstudyWorkload.Configuration(UnderstudySamples.Understudy(spec), UnderstudySamples.Production(vault: true), UnderstudyJobKinds.Keys);
    }

    void Because()
    {
        _json = _configuration.ToJson();
        _read = UnderstudyJobConfiguration.FromJson(_json);
    }

    [Fact] void should_keep_the_kind() => _read.Kind.ShouldEqual("keys");
    [Fact] void should_keep_the_retention() => _read.Retention.ShouldEqual(new RetentionPolicy(Monthly: 6));
    [Fact] void should_keep_the_buckets() => (_read.Storage.Bucket, _read.KeysStorage!.Bucket).ShouldEqual(("ledger-backups", "ledger-keys"));
    [Fact] void should_keep_the_vault() => (_read.HasVault, _read.VaultAddress).ShouldEqual((true, "https://vault.example:8200"));
    [Fact] void should_not_carry_a_credential() => _json.Contains("secret-key", StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_not_carry_a_secret_name_that_holds_a_value() => _json.Contains("secret-key-value", StringComparison.Ordinal).ShouldBeFalse();
}
#endif
