// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionVaultValidator;

public class when_the_vault_is_invalid : Specification
{
    IReadOnlyList<string> _unknownMode;
    IReadOnlyList<string> _externalWithoutAddress;
    IReadOnlyList<string> _unknownTier;
    IReadOnlyList<string> _unknownUnseal;
    IReadOnlyList<string> _cloudWithoutSettings;
    IReadOnlyList<string> _cloudWithUnknownSetting;
    IReadOnlyList<string> _cloudWithInjection;
    IReadOnlyList<string> _transitWithoutToken;

    void Because()
    {
        _unknownMode = Validate(spec => spec.Vault.Mode = "Hosted");
        _externalWithoutAddress = Validate(spec => spec.Vault.Mode = ProductionModes.External);
        _unknownTier = Validate(spec => Managed(spec).Tier = "Staging");
        _unknownUnseal = Validate(spec => Managed(spec).Unseal.Mode = "Magic");
        _cloudWithoutSettings = Validate(spec => Managed(spec).Unseal = new() { Mode = ProductionVaultUnseal.CloudKeyService, Cloud = new() { Provider = VaultCloudProviders.Aws } });
        _cloudWithUnknownSetting = Validate(spec => Managed(spec).Unseal = new()
        {
            Mode = ProductionVaultUnseal.CloudKeyService,
            Cloud = new() { Provider = VaultCloudProviders.Aws, Settings = new Dictionary<string, string> { ["region"] = "eu-west-1", ["kmsKeyId"] = "k", ["endpoint"] = "x" } }
        });
        _cloudWithInjection = Validate(spec => Managed(spec).Unseal = new()
        {
            Mode = ProductionVaultUnseal.CloudKeyService,
            Cloud = new() { Provider = VaultCloudProviders.Aws, Settings = new Dictionary<string, string> { ["region"] = "eu\" } seal \"x", ["kmsKeyId"] = "k" } }
        });
        _transitWithoutToken = Validate(spec => Managed(spec).Unseal = new()
        {
            Mode = ProductionVaultUnseal.TransitVault,
            Transit = new() { Address = "https://vault.example.com", KeyName = "unseal" }
        });
    }

    static ProductionManagedVault Managed(ProductionSpec spec)
    {
        spec.Vault.Mode = ProductionModes.Managed;
        return spec.Vault.Managed;
    }

    static IReadOnlyList<string> Validate(Action<ProductionSpec> change)
    {
        var spec = ProductionSamples.External();
        change(spec);
        return ProductionValidator.Validate(spec);
    }

    [Fact] void should_refuse_an_unknown_mode() => _unknownMode.Single().ShouldContain("vault.mode must be None, Managed or External");
    [Fact] void should_require_an_address_and_token_for_an_external_vault() => _externalWithoutAddress.Count.ShouldEqual(2);
    [Fact] void should_refuse_an_unknown_tier() => _unknownTier.Single().ShouldContain("vault.managed.tier");
    [Fact] void should_refuse_an_unknown_unseal_mode() => _unknownUnseal.Single().ShouldContain("vault.managed.unseal.mode");
    [Fact] void should_name_every_missing_cloud_setting() => _cloudWithoutSettings.Count.ShouldEqual(2);
    [Fact] void should_refuse_a_setting_the_provider_does_not_have() => _cloudWithUnknownSetting.Single().ShouldContain("settings.endpoint is not a setting of Aws");
    [Fact] void should_refuse_a_value_that_could_end_its_string() => _cloudWithInjection.Single().ShouldContain("settings.region holds a character that is not allowed");
    [Fact] void should_require_a_token_for_a_transit_vault() => _transitWithoutToken.Single().ShouldContain("transit.tokenSecretRef");
}
#endif
