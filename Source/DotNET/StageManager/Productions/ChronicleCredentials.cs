// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The credentials Chronicle bootstraps on start: its administrator and the client applications connect as. Generated once and kept in a Secret.
/// </summary>
/// <param name="ClientId">The client applications connect as.</param>
/// <param name="ClientSecret">The secret of that client.</param>
/// <param name="AdminPassword">The initial password of the administrator.</param>
public record ChronicleCredentials(string ClientId, string ClientSecret, string AdminPassword)
{
    /// <summary>
    /// The key of the client id, in the credentials and the connection Secret.
    /// </summary>
    public const string ClientIdKey = "clientId";

    /// <summary>
    /// The key of the client secret, in the credentials and the connection Secret.
    /// </summary>
    public const string ClientSecretKey = "clientSecret";

    /// <summary>
    /// The key of the administrator password in the credentials Secret.
    /// </summary>
    public const string AdminPasswordKey = "adminPassword";

    /// <summary>
    /// The user name of the administrator.
    /// </summary>
    public const string AdminUserName = "admin";

    /// <summary>
    /// The client id applications connect as.
    /// </summary>
    public const string ApplicationsClientId = "showrunner-applications";

    /// <summary>
    /// Generates new credentials.
    /// </summary>
    /// <returns>The credentials.</returns>
    public static ChronicleCredentials Generate() => new(ApplicationsClientId, GeneratedSecret.New(), GeneratedSecret.New());

    /// <summary>
    /// Gets the credentials as the values of their Secret.
    /// </summary>
    /// <returns>The values by key.</returns>
    public IReadOnlyDictionary<string, string> ToSecretValues() => new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [ClientIdKey] = ClientId,
        [ClientSecretKey] = ClientSecret,
        [AdminPasswordKey] = AdminPassword
    };
}
