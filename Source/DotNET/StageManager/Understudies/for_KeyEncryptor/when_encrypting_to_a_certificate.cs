// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Cratis.Showrunner.StageManager.Understudies.Keys;

namespace Cratis.Showrunner.StageManager.Understudies.for_KeyEncryptor;

public class when_encrypting_to_a_certificate : Specification
{
    X509Certificate2 _withPrivateKey;
    X509Certificate2 _stranger;
    byte[] _encrypted;
    string _pem;
    Exception? _strangerDecrypting;
    string _decrypted;
    string? _algorithm;

    void Establish()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=customer", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var created = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        _withPrivateKey = X509CertificateLoader.LoadPkcs12(created.Export(X509ContentType.Pfx), null);
        _pem = created.ExportCertificatePem();

        using var other = RSA.Create(2048);
        using var otherCreated = new CertificateRequest("CN=stranger", other, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1).CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        _stranger = X509CertificateLoader.LoadPkcs12(otherCreated.Export(X509ContentType.Pfx), null);
    }

    void Because()
    {
        using var publicOnly = KeyEncryptor.CertificateFromPem(_pem);
        _encrypted = KeyEncryptor.Encrypt(Encoding.UTF8.GetBytes("the keys"), publicOnly);

        var envelope = new EnvelopedCms();
        envelope.Decode(_encrypted);
        _strangerDecrypting = Catch.Exception(() => envelope.Decrypt(new X509Certificate2Collection(_stranger)));

        var own = new EnvelopedCms();
        own.Decode(_encrypted);
        own.Decrypt(new X509Certificate2Collection(_withPrivateKey));
        _decrypted = Encoding.UTF8.GetString(own.ContentInfo.Content);
        _algorithm = own.ContentEncryptionAlgorithm.Oid.Value;
    }

    [Fact] void should_decrypt_with_the_private_key_of_the_customer() => _decrypted.ShouldEqual("the keys");
    [Fact] void should_not_decrypt_for_anyone_else() => _strangerDecrypting.ShouldNotBeNull();
    [Fact] void should_not_contain_the_plaintext() => Encoding.UTF8.GetString(_encrypted).Contains("the keys", StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_encrypt_with_aes_256() => _algorithm.ShouldEqual("2.16.840.1.101.3.4.1.42");
    [Fact] void should_not_need_a_private_key_to_encrypt() => KeyEncryptor.CertificateFromPem(_pem).HasPrivateKey.ShouldBeFalse();
}
#endif
