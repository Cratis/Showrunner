// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// How a managed Vault is unsealed.
/// </summary>
public sealed class VaultUnsealArgs
{
    /// <summary>
    /// Gets how Vault is unsealed. Defaults to <see cref="VaultUnsealMode.KubernetesSecret"/>.
    /// </summary>
    public VaultUnsealMode Mode { get; init; } = VaultUnsealMode.KubernetesSecret;

    /// <summary>
    /// Gets the cloud key service, for <see cref="VaultUnsealMode.CloudKeyService"/>.
    /// </summary>
    public CloudKeyServiceArgs? Cloud { get; init; }

    /// <summary>
    /// Gets the transit Vault, for <see cref="VaultUnsealMode.TransitVault"/>.
    /// </summary>
    public TransitVaultArgs? Transit { get; init; }

    /// <summary>
    /// Builds the spec of the unseal settings.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .PutEnum("mode", Mode)
            .Put("cloud", Cloud?.ToSpec())
            .Put("transit", Transit?.ToSpec());
}
