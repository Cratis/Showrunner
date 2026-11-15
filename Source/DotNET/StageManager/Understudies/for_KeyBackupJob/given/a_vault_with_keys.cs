// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.Discovery;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.StageManager.Understudies.Keys;

namespace Cratis.Showrunner.StageManager.Understudies.for_KeyBackupJob.given;

/// <summary>
/// A Vault that holds the keys and erasure fences of two namespaces of one event store, and a customer whose public certificate the keys are encrypted to.
/// </summary>
public abstract class a_vault_with_keys : Specification
{
    protected static readonly DateTimeOffset _now = new(2026, 3, 15, 2, 0, 0, TimeSpan.Zero);

    protected const string AcmeKey = "acme-key-material-4f1c";
    protected const string GlobexKey = "globex-key-material-99ab";

    protected InMemoryObjectStorage _storage;
    protected FakeVaultKeyReader _vault;
    protected X509Certificate2 _customer;
    protected X509Certificate2 _publicCertificate;
    protected UnderstudyJobConfiguration _configuration;
    protected KeyBackupResult _result;

    protected string Prefix(string eventStore, string @namespace) => BackupLayout.RunPrefix("ledger", eventStore, @namespace, _now);

    protected string DecryptedKeys(string eventStore, string @namespace) => Decrypt(_storage.ContentOf(Prefix(eventStore, @namespace) + KeyBackupJob.KeysFile));

    protected string Decrypt(byte[] message)
    {
        var envelope = new EnvelopedCms();
        envelope.Decode(message);
        envelope.Decrypt(new X509Certificate2Collection(_customer));
        return Encoding.UTF8.GetString(envelope.ContentInfo.Content);
    }

    void Establish()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=customer", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var created = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        _customer = X509CertificateLoader.LoadPkcs12(created.Export(X509ContentType.Pfx), null);
        _publicCertificate = KeyEncryptor.CertificateFromPem(created.ExportCertificatePem());

        _storage = new(new FixedTimeProvider(_now));
        _vault = new();
        _vault.Secrets["Studio/acme"] =
        [
            new() { Path = "Studio/acme/keys/1", Data = new Dictionary<string, string> { ["key"] = AcmeKey } },
            new() { Path = "Studio/acme/fences/subject-7", Data = new Dictionary<string, string> { ["erased"] = "true" } }
        ];
        _vault.Secrets["Studio/globex"] = [new() { Path = "Studio/globex/keys/1", Data = new Dictionary<string, string> { ["key"] = GlobexKey } }];

        _configuration = new(UnderstudyJobKinds.Keys, "ledger", "19.148.0", string.Empty, [], true, new RetentionPolicy(), new(), new(), 35, "https://vault.example:8200");
    }

    protected async Task Run(bool hasVault = true, FakeChronicleDirectory? directory = null)
    {
        var job = new KeyBackupJob(
            _storage,
            directory ?? new FakeChronicleDirectory(new([new("Studio", ["acme", "globex"])])),
            _vault,
            _publicCertificate,
            new FixedTimeProvider(_now),
            Substitute.For<ILogger<KeyBackupJob>>());
        _result = await job.RunAsync(_configuration with { HasVault = hasVault }, CancellationToken.None);
    }
}
#endif
