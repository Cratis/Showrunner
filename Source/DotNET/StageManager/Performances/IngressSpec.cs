// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// How an application is reached from outside the cluster. AuthProxy is the only public entry point.
/// </summary>
public class IngressSpec
{
    /// <summary>
    /// Gets or sets whether an Ingress is created.
    /// </summary>
    [Description("Whether an Ingress is created.")]
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the ingress class.
    /// </summary>
    [Description("The ingress class.")]
    public string? ClassName { get; set; }

    /// <summary>
    /// Gets or sets the public host names. The first one is the public URL.
    /// </summary>
    [Description("The public host names. The first one is the public URL.")]
    public IList<string> Hosts { get; set; } = [];

    /// <summary>
    /// Gets or sets the cert-manager issuer that issues the certificate.
    /// </summary>
    [Description("The cert-manager issuer that issues the certificate.")]
    public TlsIssuerReference? TlsIssuer { get; set; }
}
