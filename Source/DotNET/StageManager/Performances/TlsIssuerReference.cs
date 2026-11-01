// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// A cert-manager issuer.
/// </summary>
public class TlsIssuerReference
{
    /// <summary>
    /// Gets or sets the name of the issuer.
    /// </summary>
    [Required]
    [Description("The name of the cert-manager issuer.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the kind of the issuer: Issuer or ClusterIssuer.
    /// </summary>
    [Description("Issuer or ClusterIssuer.")]
    public string Kind { get; set; } = "ClusterIssuer";
}
