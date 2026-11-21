// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// The Cratis environment a Performance runs: the environment file the operator resolves for the application.
/// </summary>
public sealed class PerformanceEnvironmentArgs
{
    /// <summary>
    /// Gets the name of the Cratis environment, for example <c>production</c>.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the name of the ConfigMap holding the environment file. It is created, under this name, when <see cref="Content"/> is
    /// given; otherwise it must already exist, for example because another Performance of the application creates it.
    /// Defaults to <c>&lt;name&gt;-environment</c> when there is content.
    /// </summary>
    public string? ConfigMapName { get; init; }

    /// <summary>
    /// Gets the key of the environment file in the ConfigMap. Defaults to <c>&lt;environment name&gt;.yaml</c>.
    /// </summary>
    public string? ConfigMapKey { get; init; }

    /// <summary>
    /// Gets the environment file as YAML text, which the component writes to the ConfigMap. The file holds plain values,
    /// encrypted values or references, never plain secrets.
    /// </summary>
    public Input<string>? Content { get; init; }

    /// <summary>
    /// Gets the name of the existing Secret holding the passphrase of a passphrase environment. Azure Key Vault and HashiCorp
    /// Vault environments need none.
    /// </summary>
    public string? PassphraseSecretName { get; init; }

    /// <summary>
    /// Gets the key of the passphrase in that Secret.
    /// </summary>
    public string? PassphraseSecretKey { get; init; }

    /// <summary>
    /// Gets the key the operator reads from the ConfigMap when none is named.
    /// </summary>
    internal string DefaultConfigMapKey => $"{Name}.yaml";

    /// <summary>
    /// Builds the spec of the environment.
    /// </summary>
    /// <param name="configMapName">The name of the ConfigMap holding the environment file.</param>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec(string configMapName) =>
        SpecExtensions.NewSpec()
            .Put("name", Name)
            .Put("configMapRef", SpecExtensions.Reference(configMapName, ConfigMapKey))
            .Put("passphraseSecretRef", PassphraseSecretName is null ? null : SpecExtensions.SecretReference(PassphraseSecretName, PassphraseSecretKey));
}
