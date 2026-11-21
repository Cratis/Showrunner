// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The Secret that holds the client id and the client secret of an identity provider.
/// </summary>
public class ProviderCredentialsReference
{
    /// <summary>
    /// Gets or sets the name of the Secret.
    /// </summary>
    [Required]
    [Description("The name of the Secret, in the namespace of the Performance.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key that holds the client id.
    /// </summary>
    [Description("The key that holds the client id. Defaults to clientId.")]
    public string ClientIdKey { get; set; } = "clientId";

    /// <summary>
    /// Gets or sets the key that holds the client secret.
    /// </summary>
    [Description("The key that holds the client secret. Defaults to clientSecret.")]
    public string ClientSecretKey { get; set; } = "clientSecret";
}
