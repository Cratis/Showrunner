// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// How a managed Vault is unsealed whenever it starts.
/// </summary>
public enum VaultUnsealMode
{
    /// <summary>
    /// A helper unseals Vault with a key kept in a Secret. The default.
    /// </summary>
    KubernetesSecret = 0,

    /// <summary>
    /// A cloud key service unseals Vault: AWS KMS, Azure Key Vault or Google Cloud KMS.
    /// </summary>
    CloudKeyService = 1,

    /// <summary>
    /// A second Vault outside the cluster unseals Vault through its transit engine.
    /// </summary>
    TransitVault = 2
}
