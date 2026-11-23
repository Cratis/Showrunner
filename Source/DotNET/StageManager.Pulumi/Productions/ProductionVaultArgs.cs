// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// How the key storage of a Production is provided.
/// </summary>
public sealed class ProductionVaultArgs
{
    /// <summary>
    /// The key of the token in the Secret the component writes when it is given a <see cref="Token"/>.
    /// </summary>
    public const string DefaultTokenKey = "token";

    /// <summary>
    /// Gets how the keys are stored. Defaults to <see cref="VaultMode.None"/>. It cannot be changed once the Production is running.
    /// </summary>
    public VaultMode Mode { get; init; } = VaultMode.None;

    /// <summary>
    /// Gets the address of an external Vault, such as <c>https://vault.example.com:8200</c>.
    /// </summary>
    public Input<string>? Address { get; init; }

    /// <summary>
    /// Gets the token Chronicle authenticates to an external Vault with. It is written, as a Pulumi secret, to the Secret named
    /// by <see cref="TokenSecretName"/> under the key <see cref="TokenSecretKey"/>.
    /// </summary>
    public Input<string>? Token { get; init; }

    /// <summary>
    /// Gets the name of the Secret holding the token. Created when <see cref="Token"/> is given, otherwise it must already exist.
    /// Defaults to <c>&lt;name&gt;-vault-token</c>.
    /// </summary>
    public string? TokenSecretName { get; init; }

    /// <summary>
    /// Gets the key of the token in that Secret. Defaults to <c>token</c>.
    /// </summary>
    public string? TokenSecretKey { get; init; }

    /// <summary>
    /// Gets the settings of a managed Vault.
    /// </summary>
    public ManagedVaultArgs? Managed { get; init; }

    /// <summary>
    /// Gets whether keys that only Chronicle's database holds keep being served and are copied into Vault as they are read.
    /// </summary>
    public bool MigrateFromDefaultStorage { get; init; }

    /// <summary>
    /// Builds the spec of the Vault.
    /// </summary>
    /// <param name="tokenSecret">The Secret holding the token, if any.</param>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec(SecretReferenceArgs? tokenSecret) =>
        SpecExtensions.NewSpec()
            .PutEnum("mode", Mode)
            .Put("address", Address)
            .Put("tokenSecretRef", tokenSecret is null ? null : SpecExtensions.SecretReference(tokenSecret.Name, tokenSecret.Key))
            .Put("managed", Managed?.ToSpec())
            .PutWhenTrue("migrateFromDefaultStorage", MigrateFromDefaultStorage);
}
