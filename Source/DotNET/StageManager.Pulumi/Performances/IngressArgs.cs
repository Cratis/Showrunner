// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// How the application is reached from outside the cluster. AuthProxy is the only public entry point. Declaring it creates the Ingress.
/// </summary>
public sealed class IngressArgs
{
    /// <summary>
    /// Gets the ingress class, or null for the cluster's default.
    /// </summary>
    public string? ClassName { get; init; }

    /// <summary>
    /// Gets the public host names. The first one is the public URL.
    /// </summary>
    public IList<string> Hosts { get; init; } = [];

    /// <summary>
    /// Gets the name of the cert-manager issuer that issues the certificate, or null to ask for none.
    /// </summary>
    public string? TlsIssuerName { get; init; }

    /// <summary>
    /// Gets the kind of the issuer: <c>Issuer</c> or <c>ClusterIssuer</c>, or null for the operator's default.
    /// </summary>
    public string? TlsIssuerKind { get; init; }

    /// <summary>
    /// Gets annotations added to the Ingress, for example proxy timeouts of the ingress controller.
    /// </summary>
    public IDictionary<string, string> Annotations { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Builds the spec of the Ingress.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("enabled", true)
            .Put("className", ClassName)
            .PutList("hosts", Hosts)
            .PutObject("tlsIssuer", Issuer())
            .PutMap("annotations", Annotations.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)));

    Dictionary<string, object> Issuer() =>
        TlsIssuerName is null
            ? SpecExtensions.NewSpec()
            : SpecExtensions.NewSpec().Put("name", TlsIssuerName).Put("kind", TlsIssuerKind);
}
