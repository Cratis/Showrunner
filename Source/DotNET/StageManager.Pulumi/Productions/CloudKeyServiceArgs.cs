// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The cloud key service that unseals a managed Vault.
/// </summary>
public sealed class CloudKeyServiceArgs
{
    /// <summary>
    /// Gets the cloud whose key service holds the key.
    /// </summary>
    public required CloudProvider Provider { get; init; }

    /// <summary>
    /// Gets the settings of the provider's seal. Aws: <c>region</c> and <c>kmsKeyId</c>. Azure: <c>vaultName</c>, <c>keyName</c>
    /// and optionally <c>tenantId</c>. Google: <c>project</c>, <c>region</c>, <c>keyRing</c> and <c>cryptoKey</c>.
    /// </summary>
    public IDictionary<string, Input<string>> Settings { get; init; } = new Dictionary<string, Input<string>>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the name of the existing Secret whose keys become environment variables of the Vault pods, such as
    /// <c>AWS_ACCESS_KEY_ID</c>, or null when the pods authenticate through workload identity.
    /// </summary>
    public string? CredentialsSecretName { get; init; }

    /// <summary>
    /// Builds the spec of the key service.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .PutEnum("provider", Provider)
            .PutMap("settings", Settings.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)))
            .Put("credentialsSecretName", CredentialsSecretName);
}
