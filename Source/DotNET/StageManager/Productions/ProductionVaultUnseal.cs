// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// How a managed Vault is unsealed.
/// </summary>
public class ProductionVaultUnseal
{
    /// <summary>
    /// A helper unseals Vault with a key kept in a Secret. The default.
    /// </summary>
    public const string KubernetesSecret = "KubernetesSecret";

    /// <summary>
    /// A cloud key service unseals Vault: AWS KMS, Azure Key Vault or Google Cloud KMS.
    /// </summary>
    public const string CloudKeyService = "CloudKeyService";

    /// <summary>
    /// A second Vault outside the cluster unseals Vault through its transit engine.
    /// </summary>
    public const string TransitVault = "TransitVault";

    /// <summary>
    /// Gets or sets how Vault is unsealed.
    /// </summary>
    [Description("KubernetesSecret (the default: the unseal key is kept in a Secret), CloudKeyService (AWS KMS, Azure Key Vault or Google Cloud KMS) or TransitVault (a second Vault outside the cluster).")]
    public string Mode { get; set; } = KubernetesSecret;

    /// <summary>
    /// Gets or sets the cloud key service.
    /// </summary>
    [Description("For CloudKeyService: the key service that holds the key. Named cloud in the resource.")]
    public ProductionVaultCloudKeyService Cloud { get; set; } = new();

    /// <summary>
    /// Gets or sets the transit Vault.
    /// </summary>
    [Description("For TransitVault: the Vault outside the cluster that holds the key.")]
    public ProductionVaultTransit Transit { get; set; } = new();
}
