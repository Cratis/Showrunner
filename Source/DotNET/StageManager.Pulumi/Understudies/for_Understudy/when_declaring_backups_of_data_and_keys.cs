// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;
using OperatorUnderstudySpec = Cratis.Showrunner.StageManager.Understudies.UnderstudySpec;
using OperatorUnderstudyValidator = Cratis.Showrunner.StageManager.Understudies.UnderstudyValidator;

namespace Cratis.Showrunner.StageManager.Pulumi.Understudies.for_Understudy;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_backups_of_data_and_keys : Specification
{
    DeclaredResources _declared;
    OperatorUnderstudySpec _spec;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Understudy(
            "stage-manager-understudy",
            new UnderstudyArgs
            {
                Name = "keldakraft",
                Namespace = PulumiSpecs.Namespace,
                ProductionName = "keldakraft",
                Schedule = "0 * * * *",
                MaxAgeDays = 35,
                Retention = new UnderstudyRetentionArgs { Hourly = 24, Daily = 7, Weekly = 4 },
                Storage = new UnderstudyStorageArgs
                {
                    Endpoint = "https://objects.example.com",
                    Region = "europe-1",
                    Bucket = "keldakraft-backups",
                    PathStyle = true,
                    AccessKey = "data-access-key",
                    SecretKey = "data-secret-key",
                },
                Keys = new UnderstudyKeysArgs
                {
                    Schedule = "0 2 * * *",
                    MaxAgeDays = 35,
                    Storage = new UnderstudyStorageArgs
                    {
                        Endpoint = "https://objects.example.com",
                        Region = "europe-1",
                        Bucket = "keldakraft-keys",
                        PathStyle = true,
                        AccessKey = "keys-access-key",
                        SecretKey = "keys-secret-key",
                    },
                    Certificate = "-----BEGIN CERTIFICATE-----",
                    VaultTokenSecret = new SecretReferenceArgs { Name = "keldakraft-vault-token", Key = "token" },
                },
            }));
        _spec = _declared.SpecOf<OperatorUnderstudySpec>("Understudy");
    }

    [Fact] void should_declare_the_understudy_in_the_namespace() => _declared.Custom("Understudy").KubernetesNamespace.ShouldEqual(PulumiSpecs.Namespace);
    [Fact] void should_back_up_the_production() => _spec.ProductionRef.ShouldEqual("keldakraft");
    [Fact] void should_back_up_every_hour() => _spec.Schedule.ShouldEqual("0 * * * *");
    [Fact] void should_keep_nothing_older_than_35_days() => _spec.MaxAgeDays.ShouldEqual(35);
    [Fact] void should_keep_the_tiers() => (_spec.Retention.Hourly, _spec.Retention.Daily, _spec.Retention.Weekly).ShouldEqual((24, 7, 4));
    [Fact] void should_write_the_data_to_the_bucket() => (_spec.Storage.Provider, _spec.Storage.Bucket, _spec.Storage.PathStyle).ShouldEqual(("S3", "keldakraft-backups", true));
    [Fact] void should_name_the_data_credentials_secret() => _spec.Storage.CredentialsSecretName.ShouldEqual("keldakraft-backup-credentials");
    [Fact] void should_write_the_keys_to_another_bucket() => _spec.Keys!.Storage.Bucket.ShouldEqual("keldakraft-keys");
    [Fact] void should_name_the_keys_credentials_secret() => _spec.Keys!.Storage.CredentialsSecretName.ShouldEqual("keldakraft-key-backup-credentials");
    [Fact] void should_name_the_certificate_secret() => (_spec.Keys!.CertificateSecret.Name, _spec.Keys.CertificateSecret.KeyOrDefault()).ShouldEqual(("keldakraft-key-backup-certificate", "certificate.pem"));
    [Fact] void should_name_the_vault_token_secret() => (_spec.Keys!.VaultTokenSecret.Name, _spec.Keys.VaultTokenSecret.KeyOrDefault()).ShouldEqual(("keldakraft-vault-token", "token"));
    [Fact] void should_be_valid_for_the_operator() => OperatorUnderstudyValidator.Validate(_spec).ShouldBeEmpty();
    [Fact] void should_write_the_three_secrets() => _declared.Secrets.Select(secret => secret.KubernetesName).ShouldContainOnly("keldakraft-backup-credentials", "keldakraft-key-backup-credentials", "keldakraft-key-backup-certificate");
    [Fact] void should_write_the_access_key_under_the_key_the_operator_reads() => _declared.StringData("keldakraft-backup-credentials", "accessKey").ShouldNotBeNull();
    [Fact] void should_write_the_secret_key_under_the_key_the_operator_reads() => _declared.StringData("keldakraft-backup-credentials", "secretKey").ShouldNotBeNull();
    [Fact] void should_write_the_certificate_under_its_key() => _declared.StringData("keldakraft-key-backup-certificate", "certificate.pem").ShouldNotBeNull();
}
#endif
