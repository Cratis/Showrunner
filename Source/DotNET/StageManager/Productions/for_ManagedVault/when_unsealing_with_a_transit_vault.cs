// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ManagedVault;

public class when_unsealing_with_a_transit_vault : Specification
{
    string _configuration;
    V1StatefulSet _vault;

    void Because()
    {
        var spec = ProductionSamples.External();
        spec.Vault.Mode = ProductionModes.Managed;
        spec.Vault.Managed.Unseal = new()
        {
            Mode = ProductionVaultUnseal.TransitVault,
            Transit = new() { Address = "https://vault.outside.example.com:8200", KeyName = "unseal", MountPath = "transit", TokenSecretRef = new() { Name = "transit-token" } }
        };
        var production = ProductionSamples.Production(spec);
        _configuration = ManagedVault.Configuration(production);
        _vault = ManagedVault.StatefulSet(production);
    }

    [Fact] void should_seal_with_transit() => _configuration.ShouldContain("seal \"transit\"");
    [Fact] void should_name_the_transit_vault() => _configuration.ShouldContain("address = \"https://vault.outside.example.com:8200\"");
    [Fact] void should_name_the_key() => _configuration.ShouldContain("key_name = \"unseal\"");
    [Fact] void should_keep_the_token_out_of_the_configuration() => _configuration.Contains("token", StringComparison.OrdinalIgnoreCase).ShouldBeFalse();
    [Fact] void should_take_the_token_from_the_secret() => _vault.Spec.Template.Spec.Containers[0].Env.Single(variable => variable.Name == "VAULT_TOKEN").ValueFrom.SecretKeyRef.Name.ShouldEqual("transit-token");
    [Fact] void should_default_the_key_of_the_token() => _vault.Spec.Template.Spec.Containers[0].Env.Single(variable => variable.Name == "VAULT_TOKEN").ValueFrom.SecretKeyRef.Key.ShouldEqual("token");
    [Fact] void should_tell_the_helper_not_to_unseal() => _vault.Spec.Template.Spec.Containers.Single(container => container.Name == "bootstrap").Env.Single(variable => variable.Name == "UNSEAL").Value.ShouldEqual("auto");
}
#endif
