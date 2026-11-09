// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The cert-manager resources that give Chronicle its TLS certificate: a self-signed Issuer signs a certificate authority, and an Issuer of that authority signs the certificate of Chronicle,
/// written as a PKCS#12 keystore for Chronicle and with the authority's certificate for clients.
/// </summary>
public static class ProductionCertificates
{
    /// <summary>
    /// The key of the keystore in the Secret of Chronicle's certificate.
    /// </summary>
    public const string KeystoreKey = "keystore.p12";

    /// <summary>
    /// The key of the certificate authority in a cert-manager Secret and in the connection Secret.
    /// </summary>
    public const string CertificateAuthorityKey = "ca.crt";

    /// <summary>
    /// The key of the keystore password in its Secret.
    /// </summary>
    public const string KeystorePasswordKey = "password";

    const string Component = "certificates";

    /// <summary>
    /// Creates every cert-manager resource, in the order they depend on each other.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>The Issuers and Certificates.</returns>
    public static IReadOnlyList<IKubernetesObject<V1ObjectMeta>> Create(Production production)
    {
        var names = ProductionNames.For(production);
        return
        [
            Issuer(production, names.SelfSignedIssuer, new JsonObject { ["selfSigned"] = new JsonObject() }),
            Certificate(production, names.CertificateAuthority, new JsonObject
            {
                ["isCA"] = true,
                ["commonName"] = $"{names.Production} Chronicle certificate authority",
                ["secretName"] = names.CertificateAuthority,
                ["duration"] = "87600h",
                ["privateKey"] = new JsonObject { ["algorithm"] = "ECDSA", ["size"] = 256 },
                ["issuerRef"] = IssuerRef(names.SelfSignedIssuer)
            }),
            Issuer(production, names.CertificateAuthority, new JsonObject { ["ca"] = new JsonObject { ["secretName"] = names.CertificateAuthority } }),
            Certificate(production, names.ChronicleTls, new JsonObject
            {
                ["secretName"] = names.ChronicleTls,
                ["commonName"] = names.ChronicleHost,
                ["dnsNames"] = new JsonArray(
                    names.Chronicle,
                    $"{names.Chronicle}.{names.Namespace}",
                    $"{names.Chronicle}.{names.Namespace}.svc",
                    names.ChronicleHost,
                    $"*.{names.ChronicleHost}"),
                ["duration"] = "8760h",
                ["renewBefore"] = "720h",
                ["usages"] = new JsonArray("server auth", "digital signature", "key encipherment"),
                ["keystores"] = new JsonObject
                {
                    ["pkcs12"] = new JsonObject
                    {
                        ["create"] = true,
                        ["passwordSecretRef"] = new JsonObject { ["name"] = names.KeystorePassword, ["key"] = KeystorePasswordKey }
                    }
                },
                ["issuerRef"] = IssuerRef(names.CertificateAuthority)
            })
        ];
    }

    static JsonObject IssuerRef(string name) => new() { ["name"] = name, ["kind"] = "Issuer", ["group"] = "cert-manager.io" };

    static CertManagerIssuer Issuer(Production production, string name, JsonObject spec) => new()
    {
        Metadata = ProductionMetadata.Create(production, name, Component),
        Spec = spec
    };

    static CertManagerCertificate Certificate(Production production, string name, JsonObject spec) => new()
    {
        Metadata = ProductionMetadata.Create(production, name, Component),
        Spec = spec
    };
}
