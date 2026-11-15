// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The cloud key services that can unseal a managed Vault, and the settings each needs.
/// </summary>
public static class VaultCloudProviders
{
    /// <summary>
    /// AWS KMS.
    /// </summary>
    public const string Aws = "Aws";

    /// <summary>
    /// Azure Key Vault.
    /// </summary>
    public const string Azure = "Azure";

    /// <summary>
    /// Google Cloud KMS.
    /// </summary>
    public const string Google = "Google";

    /// <summary>
    /// Gets the settings each provider requires.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string[]> Required = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        [Aws] = ["region", "kmsKeyId"],
        [Azure] = ["vaultName", "keyName"],
        [Google] = ["project", "region", "keyRing", "cryptoKey"]
    };

    /// <summary>
    /// Gets the settings each provider accepts, besides those it requires.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string[]> Optional = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        [Aws] = [],
        [Azure] = ["tenantId"],
        [Google] = []
    };
}
