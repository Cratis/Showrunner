// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Understudies;

/// <summary>
/// How the keys of a Production's Vault are backed up: exported per namespace, encrypted to a public certificate, into a bucket
/// of their own. The private key never enters the cluster.
/// </summary>
public sealed class UnderstudyKeysArgs
{
    /// <summary>
    /// The key of the public certificate in the Secret the component writes when it is given a <see cref="Certificate"/>.
    /// </summary>
    public const string DefaultCertificateKey = "certificate.pem";

    /// <summary>
    /// Gets the cron schedule of the key backups, or null for the operator's default (every day at 02:00).
    /// </summary>
    public string? Schedule { get; init; }

    /// <summary>
    /// Gets how long key backups are kept, in days, or null for the operator's default (35).
    /// </summary>
    public int? MaxAgeDays { get; init; }

    /// <summary>
    /// Gets the bucket the key backups are written to. It must not be the bucket of the data backups, and has its own credentials.
    /// </summary>
    public required UnderstudyStorageArgs Storage { get; init; }

    /// <summary>
    /// Gets the public certificate, in PEM form, the keys are encrypted to. It is written to the Secret named by
    /// <see cref="CertificateSecretName"/>. Only the public part: keep the private key out of the cluster.
    /// </summary>
    public Input<string>? Certificate { get; init; }

    /// <summary>
    /// Gets the name of the Secret holding the certificate. Created when <see cref="Certificate"/> is given, otherwise it must
    /// already exist. Defaults to <c>&lt;name&gt;-key-backup-certificate</c>.
    /// </summary>
    public string? CertificateSecretName { get; init; }

    /// <summary>
    /// Gets the key of the certificate in that Secret. Defaults to <c>certificate.pem</c>.
    /// </summary>
    public string? CertificateSecretKey { get; init; }

    /// <summary>
    /// Gets the existing Secret that holds the token the Vault is read with, such as the one a <c>Production</c> writes for its
    /// external Vault. The token needs read and list on <c>secret/</c> and, for snapshots, read on <c>sys/storage/raft/snapshot</c>.
    /// </summary>
    public required SecretReferenceArgs VaultTokenSecret { get; init; }

    /// <summary>
    /// Builds the spec of the key backups.
    /// </summary>
    /// <param name="storageCredentialsSecretName">The name of the Secret holding the keys bucket credentials, if any.</param>
    /// <param name="certificateSecret">The Secret holding the certificate.</param>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec(string? storageCredentialsSecretName, SecretReferenceArgs certificateSecret) =>
        SpecExtensions.NewSpec()
            .Put("schedule", Schedule)
            .Put("maxAgeDays", MaxAgeDays)
            .Put("storage", Storage.ToSpec(storageCredentialsSecretName))
            .Put("certificateSecret", SpecExtensions.Reference(certificateSecret.Name, certificateSecret.Key))
            .Put("vaultTokenSecret", SpecExtensions.Reference(VaultTokenSecret.Name, VaultTokenSecret.Key));
}
