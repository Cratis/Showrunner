// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The Vault outside the cluster that unseals a managed Vault through its transit engine.
/// </summary>
public class ProductionVaultTransit
{
    /// <summary>
    /// Gets or sets the address of the transit Vault.
    /// </summary>
    [Description("The address of the Vault that holds the transit key.")]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the transit key.
    /// </summary>
    [Description("The name of the transit key.")]
    public string KeyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mount path of the transit engine.
    /// </summary>
    [Description("The mount path of the transit engine. Defaults to transit.")]
    public string MountPath { get; set; } = "transit";

    /// <summary>
    /// Gets or sets the Secret holding the token for the transit Vault.
    /// </summary>
    [Description("The Secret holding the token that may use the transit key, under the key token unless a key is named.")]
    public ProductionSecretReference? TokenSecretRef { get; set; }
}
