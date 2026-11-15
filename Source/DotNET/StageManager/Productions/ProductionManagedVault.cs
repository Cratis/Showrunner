// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The settings of a Vault that Stage Manager runs for a <see cref="Production"/>.
/// </summary>
public class ProductionManagedVault
{
    /// <summary>
    /// The tier with one node and file storage.
    /// </summary>
    public const string Development = "Development";

    /// <summary>
    /// The tier with three nodes and Raft storage.
    /// </summary>
    public const string ProductionTier = "Production";

    /// <summary>
    /// The Vault image a managed Vault runs when none is given.
    /// </summary>
    public const string DefaultImage = "hashicorp/vault:1.18";

    /// <summary>
    /// Gets or sets the tier: <c>Development</c> or <c>Production</c>.
    /// </summary>
    [Description("Development (one node, file storage, Chronicle uses the root token) or Production (three nodes, Raft storage, Chronicle uses a token that can reach only its own key paths). Cannot be changed once the Production is running.")]
    public string Tier { get; set; } = Development;

    /// <summary>
    /// Gets or sets the Vault image.
    /// </summary>
    [Description("The Vault image. Defaults to a pinned version.")]
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the size of the volume of each node.
    /// </summary>
    [Description("The size of the volume of each Vault node. Defaults to 1Gi.")]
    public string StorageSize { get; set; } = "1Gi";

    /// <summary>
    /// Gets or sets the storage class of the volumes.
    /// </summary>
    [Description("The storage class of the volumes. Empty uses the cluster's default.")]
    public string StorageClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets how Vault is unsealed.
    /// </summary>
    [Description("How Vault is unsealed whenever it starts.")]
    public ProductionVaultUnseal Unseal { get; set; } = new();

    /// <summary>
    /// Gets the image a managed Vault runs.
    /// </summary>
    /// <returns>The configured image, or the pinned default.</returns>
    public string ImageOrDefault() => string.IsNullOrWhiteSpace(Image) ? DefaultImage : Image;

    /// <summary>
    /// Gets whether this is the production tier.
    /// </summary>
    /// <returns>True for the production tier.</returns>
    public bool IsProductionTier() => string.Equals(Tier, ProductionTier, StringComparison.Ordinal);

    /// <summary>
    /// Gets the number of Vault nodes the tier runs.
    /// </summary>
    /// <returns>Three for the production tier, otherwise one.</returns>
    public int Nodes() => IsProductionTier() ? 3 : 1;
}
