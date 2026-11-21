// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The settings of a Vault that Stage Manager runs for a Production.
/// </summary>
public sealed class ManagedVaultArgs
{
    /// <summary>
    /// Gets the tier, or null for the operator's default (<see cref="VaultTier.Development"/>). It cannot be changed once the Production is running.
    /// </summary>
    public VaultTier? Tier { get; init; }

    /// <summary>
    /// Gets the Vault image, or null for the operator's pinned version.
    /// </summary>
    public string? Image { get; init; }

    /// <summary>
    /// Gets the size of the volume of each Vault node, such as <c>5Gi</c>.
    /// </summary>
    public string? StorageSize { get; init; }

    /// <summary>
    /// Gets the storage class of the volumes, or null for the cluster's default.
    /// </summary>
    public string? StorageClassName { get; init; }

    /// <summary>
    /// Gets how Vault is unsealed, or null for the operator's default (a key kept in a Secret).
    /// </summary>
    public VaultUnsealArgs? Unseal { get; init; }

    /// <summary>
    /// Builds the spec of the managed Vault.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .PutEnum("tier", Tier)
            .Put("image", Image)
            .Put("storageSize", StorageSize)
            .Put("storageClassName", StorageClassName)
            .Put("unseal", Unseal?.ToSpec());
}
