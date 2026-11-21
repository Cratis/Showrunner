// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The object storage Percona Backup for MongoDB writes to, and when. It is attached with the PBM <c>minio</c> storage type, which works against any S3 compatible
/// storage: PBM's own <c>s3</c> type fails against some providers (UpCloud Object Storage answers its uploads with <c>XAmzContentSHA256Mismatch</c>).
/// </summary>
public class ProductionDatabaseBackupStorage
{
    /// <summary>
    /// The prefix the backups are kept under when none is given.
    /// </summary>
    public const string DefaultPrefix = "pbm";

    /// <summary>
    /// The key of the access key id in the credentials Secret, the name Percona Backup for MongoDB reads.
    /// </summary>
    public const string AccessKeyIdKey = "AWS_ACCESS_KEY_ID";

    /// <summary>
    /// The key of the secret access key in the credentials Secret, the name Percona Backup for MongoDB reads.
    /// </summary>
    public const string SecretAccessKeyKey = "AWS_SECRET_ACCESS_KEY";

    /// <summary>
    /// The name of the storage in the Percona resource.
    /// </summary>
    public const string StorageName = "object-storage";

    /// <summary>
    /// Gets or sets the bucket the backups are written to. Leave it empty to attach no storage.
    /// </summary>
    [Description("The bucket Percona Backup for MongoDB writes to. Empty attaches no storage, so nothing is backed up by Percona.")]
    public string Bucket { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the endpoint of the object storage.
    /// </summary>
    [Description("The endpoint of the S3 compatible storage, with or without http:// or https://. Without a scheme it is reached over TLS.")]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the region of the bucket.
    /// </summary>
    [Description("The region of the bucket. Empty leaves it to the storage.")]
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the prefix the backups are kept under in the bucket.
    /// </summary>
    [Description("The prefix the backups are kept under in the bucket. Defaults to pbm.")]
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the Secret holding the credentials.
    /// </summary>
    [Description("The Secret, in the namespace of the Production, with the keys AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY.")]
    public string CredentialsSecretName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schedules of the backups. Empty uses hourly (24 kept), daily (7), weekly (4) and monthly (12).
    /// </summary>
    [Description("When backups are taken and how many of each are kept. Empty uses hourly (24 kept), daily (7), weekly (4) and monthly (12).")]
    public IList<ProductionDatabaseBackupTask> Tasks { get; set; } = [];

    /// <summary>
    /// Gets whether a storage is attached.
    /// </summary>
    /// <returns>True when a bucket is named.</returns>
    public bool IsConfigured() => !string.IsNullOrWhiteSpace(Bucket);

    /// <summary>
    /// Gets the prefix with the default applied.
    /// </summary>
    /// <returns>The prefix.</returns>
    public string PrefixOrDefault() => string.IsNullOrWhiteSpace(Prefix) ? DefaultPrefix : Prefix.Trim().Trim('/');

    /// <summary>
    /// Gets the tasks with the default applied.
    /// </summary>
    /// <returns>The tasks.</returns>
    public IReadOnlyList<ProductionDatabaseBackupTask> TasksOrDefault() => Tasks.Count > 0 ? [.. Tasks] : ProductionDatabaseBackupTask.Defaults;

    /// <summary>
    /// Splits the endpoint into the host the <c>minio</c> storage type expects, without a scheme, and whether the connection is secure.
    /// </summary>
    /// <returns>The host, with an optional port, and whether TLS is used.</returns>
    public (string Host, bool Secure) SplitEndpoint()
    {
        const string Https = "https://";
        const string Http = "http://";
        var trimmed = Endpoint.Trim().TrimEnd('/');
        if (trimmed.StartsWith(Https, StringComparison.OrdinalIgnoreCase))
        {
            return (trimmed[Https.Length..], true);
        }

        if (trimmed.StartsWith(Http, StringComparison.OrdinalIgnoreCase))
        {
            return (trimmed[Http.Length..], false);
        }

        return (trimmed, true);
    }
}
