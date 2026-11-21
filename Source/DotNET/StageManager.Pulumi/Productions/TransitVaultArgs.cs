// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The Vault outside the cluster that unseals a managed Vault through its transit engine.
/// </summary>
public sealed class TransitVaultArgs
{
    /// <summary>
    /// Gets the address of the Vault that holds the transit key.
    /// </summary>
    public required Input<string> Address { get; init; }

    /// <summary>
    /// Gets the name of the transit key.
    /// </summary>
    public required string KeyName { get; init; }

    /// <summary>
    /// Gets the mount path of the transit engine, or null for the operator's default (<c>transit</c>).
    /// </summary>
    public string? MountPath { get; init; }

    /// <summary>
    /// Gets the existing Secret holding the token that may use the transit key, under the key <c>token</c> unless another is named.
    /// </summary>
    public SecretReferenceArgs? TokenSecret { get; init; }

    /// <summary>
    /// Builds the spec of the transit Vault.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("address", Address)
            .Put("keyName", KeyName)
            .Put("mountPath", MountPath)
            .Put("tokenSecretRef", TokenSecret is null ? null : SpecExtensions.SecretReference(TokenSecret.Name, TokenSecret.Key));
}
