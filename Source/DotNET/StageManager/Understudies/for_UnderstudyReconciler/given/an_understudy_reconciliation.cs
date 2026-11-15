// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Cratis.Showrunner.StageManager.Resources;
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.StageManager.Understudies.Reporting;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;

/// <summary>
/// The base of the Understudy reconciliation specifications: an Understudy for a Production that has written its connection Secret, with the Secrets it names in place.
/// </summary>
public abstract class an_understudy_reconciliation : Specification
{
    protected static readonly DateTimeOffset _now = new(2026, 3, 15, 12, 30, 0, TimeSpan.Zero);

    protected Understudy _understudy;
    protected FakeUnderstudyCluster _cluster;
    protected FakeObjectStorageFactory _storages;
    protected RecordingEventSink _sink;
    protected FixedTimeProvider _time;
    protected UnderstudyPhase _phase;

    protected virtual UnderstudySpec CreateSpec() => UnderstudySamples.Spec();

    protected virtual bool Vault => false;

    protected InMemoryObjectStorage Data => _storages.Bucket("ledger-backups");

    protected InMemoryObjectStorage Keys => _storages.Bucket("ledger-keys");

    protected string StatusAsJson => JsonSerializer.Serialize(_understudy.Status);

    protected ResourceCondition Condition(string type) => _understudy.Status.Conditions.Single(condition => condition.Type == type);

    protected V1CronJob? BackupCronJob => _cluster.AppliedNamed<V1CronJob>("ledger-backups-backup");

    protected V1CronJob? KeysCronJob => _cluster.AppliedNamed<V1CronJob>("ledger-backups-keys");

    protected static V1Job Job(string name, string kind, bool succeeded, DateTimeOffset started, DateTimeOffset finished) => new()
    {
        Metadata = new()
        {
            Name = name,
            NamespaceProperty = "apps",
            Labels = new Dictionary<string, string> { [UnderstudyMetadata.UnderstudyLabel] = UnderstudySamples.Name, [UnderstudyMetadata.KindLabel] = kind }
        },
        Status = new()
        {
            Succeeded = succeeded ? 1 : null,
            Failed = succeeded ? null : 1,
            StartTime = started.UtcDateTime,
            CompletionTime = succeeded ? finished.UtcDateTime : null,
            Conditions = succeeded ? [] : [new V1JobCondition { Type = "Failed", Status = "True", Reason = "BackoffLimitExceeded", LastTransitionTime = finished.UtcDateTime }]
        }
    };

    protected static string CertificatePem()
    {
        using var rsa = RSA.Create(2048);
        using var certificate = new CertificateRequest("CN=customer", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1).CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        return certificate.ExportCertificatePem();
    }

    protected virtual void Prepare()
    {
    }

    protected async Task Reconcile()
    {
        var metrics = new UnderstudyMetrics();
        var reconciler = new UnderstudyReconciler(
            _cluster,
            _storages,
            new UnderstudyReporter([_sink], metrics),
            new UnderstudyOptions("ghcr.io/cratis/showrunner-understudy:1.2.3"),
            _time,
            Substitute.For<ILogger<UnderstudyReconciler>>());
        _phase = await reconciler.ReconcileAsync(_understudy, CancellationToken.None);
    }

    void Establish()
    {
        _understudy = UnderstudySamples.Understudy(CreateSpec());
        _cluster = new() { Production = UnderstudySamples.Production(Vault) };
        _storages = new();
        _sink = new();
        _time = new(_now);
        _cluster.AddSecret("backup-credentials", "accessKey", "access-key-value");
        _cluster.AddSecret("backup-credentials", "secretKey", "secret-key-value");
        if (_understudy.Spec.Keys is not null)
        {
            _cluster.AddSecret("keys-credentials", "accessKey", "keys-access-value");
            _cluster.AddSecret("keys-credentials", "secretKey", "keys-secret-value");
            _cluster.AddSecret("customer-certificate", "certificate.pem", CertificatePem());
            _cluster.AddSecret("vault-reader", "token", "vault-token-value");
        }

        Prepare();
    }
}
#endif
