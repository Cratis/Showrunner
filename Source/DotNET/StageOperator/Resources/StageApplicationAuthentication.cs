// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents the sign-in proxy that runs in front of Stage.</summary>
public record StageApplicationAuthentication
{
    /// <summary>Gets the proxy image.</summary>
    [JsonPropertyName("proxyImage")]
    public string ProxyImage { get; init; } = string.Empty;

    /// <summary>Gets the name of the secret that configures the proxy.</summary>
    [JsonPropertyName("configSecretRef")]
    public string ConfigSecretRef { get; init; } = string.Empty;

    /// <summary>Gets the port the proxy listens on.</summary>
    [JsonPropertyName("port")]
    public int Port { get; init; } = 8080;
}
