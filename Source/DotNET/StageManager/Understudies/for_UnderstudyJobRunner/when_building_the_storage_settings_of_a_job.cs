// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyJobRunner;

public class when_building_the_storage_settings_of_a_job : Specification
{
    ObjectStorageSettings _data;
    ObjectStorageSettings _keys;
    ObjectStorageSettings _azure;

    void Because()
    {
        var spec = UnderstudySamples.Keys();
        var environment = new Dictionary<string, string>
        {
            ["UNDERSTUDY_STORAGE_ACCESS_KEY"] = "data-access",
            ["UNDERSTUDY_STORAGE_SECRET_KEY"] = "data-secret",
            ["UNDERSTUDY_KEYS_STORAGE_ACCESS_KEY"] = "keys-access",
            ["UNDERSTUDY_KEYS_STORAGE_SECRET_KEY"] = "keys-secret",
            ["UNDERSTUDY_AZ_ACCOUNT_NAME"] = "account",
            ["UNDERSTUDY_AZ_ACCOUNT_KEY"] = "account-key"
        };
        _data = UnderstudyJobRunner.SettingsOf(UnderstudySamples.Spec().Storage, UnderstudyJobEnvironment.StoragePrefix, environment);
        _keys = UnderstudyJobRunner.SettingsOf(spec.Storage, UnderstudyJobEnvironment.KeysStoragePrefix, environment);
        _azure = UnderstudyJobRunner.SettingsOf(new() { Provider = ObjectStorageProviders.AzureBlob, Bucket = "container" }, "UNDERSTUDY_AZ_", environment);
    }

    [Fact] void should_take_the_data_credentials_for_the_data_bucket() => (_data.AccessKey, _data.SecretKey).ShouldEqual(("data-access", "data-secret"));
    [Fact] void should_take_the_keys_credentials_for_the_keys_bucket() => (_keys.AccessKey, _keys.SecretKey).ShouldEqual(("keys-access", "keys-secret"));
    [Fact] void should_not_let_the_buckets_share_credentials() => _data.AccessKey.ShouldNotEqual(_keys.AccessKey);
    [Fact] void should_take_the_endpoint_from_the_spec() => (_data.Endpoint, _data.Region, _data.PathStyle).ShouldEqual(("https://objects.example.com", "se-sto1", true));
    [Fact] void should_take_the_account_of_azure_blob() => (_azure.AccountName, _azure.AccountKey, _azure.Bucket).ShouldEqual(("account", "account-key", "container"));
}
#endif
