// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// One identity provider the application signs users in with.
/// </summary>
public class AuthProxyProvider
{
    /// <summary>
    /// Gets or sets the kind of provider.
    /// </summary>
    [Required]
    [Description("EntraId, GitHub, Google, Apple or OpenId.")]
    public AuthProxyProviderKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the identity: the model's alias, otherwise the provider name. Its configuration lives in the environment under authentication.providers.IDENTITY.
    /// </summary>
    [Required]
    [Description("The model's alias, otherwise the provider name. Letters, digits, spaces, hyphens and underscores; unique ignoring case.")]
    public string Identity { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Secret that holds the client id and the client secret. Omit to take them from the Cratis environment.
    /// </summary>
    [Description("The Secret that holds the client id and the client secret. Omit to take them from the Cratis environment.")]
    public ProviderCredentialsReference? CredentialsSecretRef { get; set; }

    /// <summary>
    /// Gets or sets the scopes the provider is asked for. Omit to take the environment's scopes key, or the provider's default.
    /// </summary>
    [Description("The scopes the provider is asked for, for example read:user, user:email and read:org for GitHub. Omit to take the scopes key of the environment, or the provider's default.")]
    public IList<string> Scopes { get; set; } = [];
}
