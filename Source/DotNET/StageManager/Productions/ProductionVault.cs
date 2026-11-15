// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// How the key storage of a <see cref="Production"/> is provided.
/// </summary>
public class ProductionVault
{
    /// <summary>
    /// Gets or sets the mode: <c>None</c>, <c>Managed</c> or <c>External</c>.
    /// </summary>
    [Description("None (Chronicle keeps its keys in its database), Managed (Stage Manager runs Vault) or External. Cannot be changed once the Production is running.")]
    public string Mode { get; set; } = ProductionModes.None;

    /// <summary>
    /// Gets or sets the address of an external Vault.
    /// </summary>
    [Description("For an External Vault: its address, such as https://vault.example.com:8200.")]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Secret holding the token Chronicle authenticates to an external Vault with.
    /// </summary>
    [Description("For an External Vault: the Secret holding the token Chronicle authenticates with (VAULT_TOKEN), under the key token unless a key is named. The token needs the policy in the Chronicle documentation on compliance storage.")]
    public ProductionSecretReference? TokenSecretRef { get; set; }

    /// <summary>
    /// Gets or sets the settings of a managed Vault.
    /// </summary>
    [Description("For a Managed Vault: its tier, storage and how it is unsealed.")]
    public ProductionManagedVault Managed { get; set; } = new();
}
