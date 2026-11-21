// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The S3 compatible object storage Percona Backup for MongoDB writes to, and when. It is attached with the PBM
/// <c>minio</c> storage type, which works against providers whose uploads PBM's own <c>s3</c> type fails on.
/// </summary>
public sealed class DatabaseBackupStorageArgs
{
    /// <summary>
    /// The key of the access key id in the credentials Secret, the name Percona Backup for MongoDB reads.
    /// </summary>
    public const string AccessKeyIdKey = "AWS_ACCESS_KEY_ID";

    /// <summary>
    /// The key of the secret access key in the credentials Secret, the name Percona Backup for MongoDB reads.
    /// </summary>
    public const string SecretAccessKeyKey = "AWS_SECRET_ACCESS_KEY";

    /// <summary>
    /// Gets the bucket the backups are written to.
    /// </summary>
    public required Input<string> Bucket { get; init; }

    /// <summary>
    /// Gets the endpoint of the storage, with or without <c>http://</c> or <c>https://</c>. Without a scheme it is reached over TLS.
    /// </summary>
    public Input<string>? Endpoint { get; init; }

    /// <summary>
    /// Gets the region of the bucket, or null to leave it to the storage.
    /// </summary>
    public Input<string>? Region { get; init; }

    /// <summary>
    /// Gets the prefix the backups are kept under in the bucket, or null for the operator's default (<c>pbm</c>).
    /// </summary>
    public string? Prefix { get; init; }

    /// <summary>
    /// Gets the name of the Secret holding the credentials. It is created, under this name, when <see cref="AccessKeyId"/>
    /// and <see cref="SecretAccessKey"/> are given; otherwise it must already exist. Defaults to <c>&lt;name&gt;-backup-credentials</c>.
    /// </summary>
    public string? CredentialsSecretName { get; init; }

    /// <summary>
    /// Gets the access key id to write into the credentials Secret, or null when the Secret exists already.
    /// </summary>
    public Input<string>? AccessKeyId { get; init; }

    /// <summary>
    /// Gets the secret access key to write into the credentials Secret, or null when the Secret exists already.
    /// </summary>
    public Input<string>? SecretAccessKey { get; init; }

    /// <summary>
    /// Gets the schedules of the backups, or none to use the operator's: hourly (24 kept), daily (7), weekly (4) and monthly (12).
    /// </summary>
    public IList<BackupTaskArgs> Tasks { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether credential values were given for the component to write.
    /// </summary>
    internal bool HasCredentials => AccessKeyId is not null || SecretAccessKey is not null;

    /// <summary>
    /// Builds the spec of the storage.
    /// </summary>
    /// <param name="credentialsSecretName">The name of the Secret holding the credentials, if any.</param>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec(string? credentialsSecretName) =>
        SpecExtensions.NewSpec()
            .Put("bucket", Bucket)
            .Put("endpoint", Endpoint)
            .Put("region", Region)
            .Put("prefix", Prefix)
            .Put("credentialsSecretName", credentialsSecretName)
            .PutList("tasks", Tasks.Select(task => task.ToSpec()));
}
