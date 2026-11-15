// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Productions;
using Cratis.Showrunner.StageManager.Understudies.Catalog;

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// How the keys of a Production are backed up: exported per namespace, encrypted to the customer's public certificate, into a bucket of their own.
/// The private key never enters the cluster.
/// </summary>
public class UnderstudyKeys
{
    /// <summary>
    /// The key of the Secret that holds the public certificate when the reference names none.
    /// </summary>
    public const string DefaultCertificateKey = "certificate.pem";

    /// <summary>
    /// The key of the Secret that holds the Vault token when the reference names none.
    /// </summary>
    public const string DefaultTokenKey = "token";

    /// <summary>
    /// Gets or sets the cron schedule of the key backups.
    /// </summary>
    [Description("The cron schedule of the key backups. Defaults to every day at 02:00.")]
    public string Schedule { get; set; } = "0 2 * * *";

    /// <summary>
    /// Gets or sets how long key backups are kept, in days.
    /// </summary>
    [Description("How long key backups are kept, in days. Defaults to 35. It is applied as an expiry rule on the keys bucket, so an erased key does not survive in a backup.")]
    public int MaxAgeDays { get; set; } = RetentionPolicy.DefaultMaxAgeDays;

    /// <summary>
    /// Gets or sets the bucket the key backups are written to. It must not be the bucket of the data backups.
    /// </summary>
    [Description("The bucket the key backups are written to, separate from the data backups and under its own credentials. Versioning must be off, object lock off, and the bucket must accept an expiry rule; a bucket that cannot prove this refuses key backups.")]
    public UnderstudyStorage Storage { get; set; } = new();

    /// <summary>
    /// Gets or sets the Secret that holds the public certificate the keys are encrypted to.
    /// </summary>
    [Description("A Secret that holds the customer's public certificate in PEM form, under the key certificate.pem unless another is named. Only the public part: keep the private key out of the cluster.")]
    public ProductionSecretReference CertificateSecret { get; set; } = new();

    /// <summary>
    /// Gets or sets the Secret that holds the token the Vault is read with.
    /// </summary>
    [Description("A Secret that holds the token the Vault is read with, under the key token unless another is named. The token needs read and list on secret/ and, for snapshots, read on sys/storage/raft/snapshot.")]
    public ProductionSecretReference VaultTokenSecret { get; set; } = new();
}
