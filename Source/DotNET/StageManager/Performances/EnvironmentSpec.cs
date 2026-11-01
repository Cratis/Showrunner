// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The Cratis environment a <see cref="Performance"/> runs.
/// </summary>
public class EnvironmentSpec
{
    /// <summary>
    /// Gets or sets the name of the environment, for example <c>production</c>.
    /// </summary>
    [Required]
    [Description("The name of the Cratis environment, for example production.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ConfigMap holding the environment file (.cratis/environments/NAME.yaml).
    /// </summary>
    [Required]
    [Description("The ConfigMap holding the environment file. The file holds plain values, encrypted values or references; never plain secrets.")]
    public ConfigMapKeyReference ConfigMapRef { get; set; } = new();

    /// <summary>
    /// Gets or sets the Secret holding the passphrase of a passphrase environment.
    /// </summary>
    [Description("The Secret holding the passphrase, for an environment whose secrets provider is the passphrase. Azure Key Vault uses workload identity and HashiCorp Vault uses the operator's Kubernetes auth; neither needs this.")]
    public SecretKeyReference? PassphraseSecretRef { get; set; }
}
