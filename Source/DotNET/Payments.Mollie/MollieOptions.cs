// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments.Mollie;

/// <summary>
/// The options for Mollie.
/// </summary>
public class MollieOptions
{
    /// <summary>
    /// The configuration section the options are read from.
    /// </summary>
    public const string SectionName = "Mollie";

    /// <summary>
    /// Gets or sets Mollie's API key. Keep it in a secret store, never in source. Without one the provider is not configured.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address of Mollie's API, including its version.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.mollie.com/v2";

    /// <summary>
    /// Gets whether an API key is set.
    /// </summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
