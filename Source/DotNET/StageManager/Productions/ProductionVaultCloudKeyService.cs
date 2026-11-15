// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The cloud key service that unseals a managed Vault.
/// </summary>
public class ProductionVaultCloudKeyService
{
    /// <summary>
    /// Gets or sets the provider: <c>Aws</c>, <c>Azure</c> or <c>Google</c>.
    /// </summary>
    [Description("Aws, Azure or Google.")]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the settings of the provider's seal.
    /// </summary>
    [Description("Aws: region and kmsKeyId. Azure: vaultName, keyName and optionally tenantId. Google: project, region, keyRing and cryptoKey.")]
    public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the Secret whose keys become environment variables of the Vault pods, as the provider's credentials.
    /// </summary>
    [Description("The Secret whose keys become environment variables of Vault, such as AWS_ACCESS_KEY_ID, AZURE_CLIENT_SECRET or GOOGLE_CREDENTIALS. Empty when the pods authenticate through workload identity.")]
    public string CredentialsSecretName { get; set; } = string.Empty;
}
