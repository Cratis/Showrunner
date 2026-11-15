// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace Cratis.Showrunner.StageManager.Understudies.Keys;

/// <summary>
/// Encrypts to the customer's public certificate as CMS (an enveloped message, AES-256), so only the holder of the private key can read it. The private key never enters the cluster.
/// </summary>
public static class KeyEncryptor
{
    const string Aes256Cbc = "2.16.840.1.101.3.4.1.42";

    /// <summary>
    /// Reads a public certificate.
    /// </summary>
    /// <param name="pem">The certificate in PEM form.</param>
    /// <returns>The certificate.</returns>
    /// <exception cref="CryptographicException">Thrown when the content is not a certificate.</exception>
    public static X509Certificate2 CertificateFromPem(string pem) => X509Certificate2.CreateFromPem(pem);

    /// <summary>
    /// Encrypts the content. The caller clears its own copy of the plaintext afterwards.
    /// </summary>
    /// <param name="plaintext">The plaintext.</param>
    /// <param name="certificate">The certificate of the recipient.</param>
    /// <returns>The CMS message.</returns>
    public static byte[] Encrypt(byte[] plaintext, X509Certificate2 certificate)
    {
        var envelope = new EnvelopedCms(new ContentInfo(plaintext), new AlgorithmIdentifier(new Oid(Aes256Cbc)));
        envelope.Encrypt(new CmsRecipient(certificate));
        return envelope.Encode();
    }
}
