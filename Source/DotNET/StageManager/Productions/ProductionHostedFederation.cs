// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The Chronicle of a Production acting as the certificate authority for the federation links of its customers.
/// </summary>
public class ProductionHostedFederation
{
    /// <summary>
    /// Gets or sets whether Chronicle acts as the authority.
    /// </summary>
    [Description("Acts as the certificate authority and accepts enrollments.")]
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the address customers reach the authority on.
    /// </summary>
    [Description("The https address customers reach the federation listener on, such as https://federation.example.com:35100.")]
    public string PublicEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the trust domain of the authority.
    /// </summary>
    [Description("The trust domain of the authority, such as example.")]
    public string TrustDomain { get; set; } = string.Empty;
}
