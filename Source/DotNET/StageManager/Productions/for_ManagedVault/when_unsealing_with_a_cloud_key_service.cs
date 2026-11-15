// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ManagedVault;

public class when_unsealing_with_a_cloud_key_service : Specification
{
    string _aws;
    string _azure;
    string _google;
    V1StatefulSet _vault;

    void Because()
    {
        _aws = Configure(VaultCloudProviders.Aws, new() { ["region"] = "eu-west-1", ["kmsKeyId"] = "arn:aws:kms:eu-west-1:111122223333:key/abc-123" }, "aws-credentials", out _vault);
        _azure = Configure(VaultCloudProviders.Azure, new() { ["vaultName"] = "keys", ["keyName"] = "unseal", ["tenantId"] = "tenant-1" }, string.Empty, out _);
        _google = Configure(VaultCloudProviders.Google, new() { ["project"] = "p", ["region"] = "global", ["keyRing"] = "ring", ["cryptoKey"] = "key" }, string.Empty, out _);
    }

    static string Configure(string provider, Dictionary<string, string> settings, string credentials, out V1StatefulSet vault)
    {
        var spec = ProductionSamples.External();
        spec.Vault.Mode = ProductionModes.Managed;
        spec.Vault.Managed.Unseal = new()
        {
            Mode = ProductionVaultUnseal.CloudKeyService,
            Cloud = new() { Provider = provider, Settings = settings, CredentialsSecretName = credentials }
        };
        var production = ProductionSamples.Production(spec);
        vault = ManagedVault.StatefulSet(production);
        return ManagedVault.Configuration(production);
    }

    [Fact] void should_seal_with_aws_kms() => _aws.ShouldContain("seal \"awskms\"");
    [Fact] void should_name_the_aws_key() => _aws.ShouldContain("kms_key_id = \"arn:aws:kms:eu-west-1:111122223333:key/abc-123\"");
    [Fact] void should_seal_with_azure_key_vault() => _azure.ShouldContain("seal \"azurekeyvault\"");
    [Fact] void should_name_the_azure_key() => _azure.ShouldContain("key_name = \"unseal\"");
    [Fact] void should_name_the_azure_tenant() => _azure.ShouldContain("tenant_id = \"tenant-1\"");
    [Fact] void should_seal_with_google_cloud_kms() => _google.ShouldContain("seal \"gcpckms\"");
    [Fact] void should_name_the_google_crypto_key() => _google.ShouldContain("crypto_key = \"key\"");
    [Fact] void should_take_the_credentials_from_the_secret() => _vault.Spec.Template.Spec.Containers[0].EnvFrom.Single().SecretRef.Name.ShouldEqual("aws-credentials");
    [Fact] void should_tell_the_helper_not_to_unseal() => _vault.Spec.Template.Spec.Containers.Single(container => container.Name == "bootstrap").Env.Single(variable => variable.Name == "UNSEAL").Value.ShouldEqual("auto");
}
#endif
