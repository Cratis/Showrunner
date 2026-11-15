// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Finds what is wrong with the <see cref="ProductionVault"/> of a Production. A setting that ends up in the Vault configuration may not hold
/// a character that could end the string it stands in, so a spec can never write configuration of its own.
/// </summary>
internal static partial class ProductionVaultValidator
{
    static readonly string[] _modes = [ProductionModes.None, ProductionModes.Managed, ProductionModes.External];
    static readonly string[] _tiers = [ProductionManagedVault.Development, ProductionManagedVault.ProductionTier];
    static readonly string[] _unsealModes = [ProductionVaultUnseal.KubernetesSecret, ProductionVaultUnseal.CloudKeyService, ProductionVaultUnseal.TransitVault];

    [GeneratedRegex("^[A-Za-z0-9._:/@=,+ -]+$", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SafeValue { get; }

    /// <summary>
    /// Validates the vault of a Production.
    /// </summary>
    /// <param name="vault">The vault.</param>
    /// <param name="problems">The problems to add to.</param>
    internal static void Validate(ProductionVault vault, List<ProductionProblem> problems)
    {
        if (!_modes.Contains(vault.Mode, StringComparer.Ordinal))
        {
            problems.Add(new($"vault.mode must be None, Managed or External, not '{vault.Mode}'."));
            return;
        }

        switch (vault.Mode)
        {
            case ProductionModes.External:
                ValidateExternal(vault, problems);
                break;
            case ProductionModes.Managed:
                ValidateManaged(vault.Managed, problems);
                break;
        }
    }

    static void ValidateExternal(ProductionVault vault, List<ProductionProblem> problems)
    {
        if (!IsHttpAddress(vault.Address))
        {
            problems.Add(new("vault.address must be the http or https address of the Vault, such as https://vault.example.com:8200, when vault.mode is External."));
        }

        if (string.IsNullOrWhiteSpace(vault.TokenSecretRef?.Name))
        {
            problems.Add(new("vault.tokenSecretRef must name the Secret holding the token Chronicle authenticates with when vault.mode is External."));
        }
    }

    static void ValidateManaged(ProductionManagedVault managed, List<ProductionProblem> problems)
    {
        if (!_tiers.Contains(managed.Tier, StringComparer.Ordinal))
        {
            problems.Add(new($"vault.managed.tier must be Development or Production, not '{managed.Tier}'."));
        }

        if (string.IsNullOrWhiteSpace(managed.StorageSize))
        {
            problems.Add(new("vault.managed.storageSize is required."));
        }

        var unseal = managed.Unseal;
        if (!_unsealModes.Contains(unseal.Mode, StringComparer.Ordinal))
        {
            problems.Add(new($"vault.managed.unseal.mode must be KubernetesSecret, CloudKeyService or TransitVault, not '{unseal.Mode}'."));
            return;
        }

        switch (unseal.Mode)
        {
            case ProductionVaultUnseal.CloudKeyService:
                ValidateCloud(unseal.Cloud, problems);
                break;
            case ProductionVaultUnseal.TransitVault:
                ValidateTransit(unseal.Transit, problems);
                break;
        }
    }

    static void ValidateCloud(ProductionVaultCloudKeyService cloud, List<ProductionProblem> problems)
    {
        if (!VaultCloudProviders.Required.TryGetValue(cloud.Provider, out var required))
        {
            problems.Add(new($"vault.managed.unseal.cloud.provider must be Aws, Azure or Google, not '{cloud.Provider}'."));
            return;
        }

        var known = required.Concat(VaultCloudProviders.Optional[cloud.Provider]).ToHashSet(StringComparer.Ordinal);
        foreach (var name in required.Where(name => !cloud.Settings.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value)))
        {
            problems.Add(new($"vault.managed.unseal.cloud.settings.{name} is required for {cloud.Provider}."));
        }

        foreach (var (name, value) in cloud.Settings)
        {
            if (!known.Contains(name))
            {
                problems.Add(new($"vault.managed.unseal.cloud.settings.{name} is not a setting of {cloud.Provider}; use {string.Join(", ", known)}."));
            }
            else if (!string.IsNullOrEmpty(value) && !SafeValue.IsMatch(value))
            {
                problems.Add(new($"vault.managed.unseal.cloud.settings.{name} holds a character that is not allowed."));
            }
        }
    }

    static void ValidateTransit(ProductionVaultTransit transit, List<ProductionProblem> problems)
    {
        if (!IsHttpAddress(transit.Address) || !SafeValue.IsMatch(transit.Address))
        {
            problems.Add(new("vault.managed.unseal.transit.address must be the http or https address of the transit Vault."));
        }

        if (string.IsNullOrWhiteSpace(transit.KeyName) || !SafeValue.IsMatch(transit.KeyName))
        {
            problems.Add(new("vault.managed.unseal.transit.keyName is required."));
        }

        if (string.IsNullOrWhiteSpace(transit.MountPath) || !SafeValue.IsMatch(transit.MountPath))
        {
            problems.Add(new("vault.managed.unseal.transit.mountPath is required."));
        }

        if (string.IsNullOrWhiteSpace(transit.TokenSecretRef?.Name))
        {
            problems.Add(new("vault.managed.unseal.transit.tokenSecretRef must name the Secret holding the token for the transit Vault."));
        }
    }

    static bool IsHttpAddress(string address) =>
        Uri.TryCreate(address, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
