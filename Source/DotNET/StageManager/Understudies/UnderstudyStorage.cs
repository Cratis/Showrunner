// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// An object storage bucket an Understudy writes to. Credentials are never part of the spec: they come from a Secret.
/// </summary>
public class UnderstudyStorage
{
    /// <summary>
    /// The key of the Secret that holds the access key of an S3-compatible provider.
    /// </summary>
    public const string AccessKeyKey = "accessKey";

    /// <summary>
    /// The key of the Secret that holds the secret key of an S3-compatible provider.
    /// </summary>
    public const string SecretKeyKey = "secretKey";

    /// <summary>
    /// The key of the Secret that holds the storage account of Azure Blob.
    /// </summary>
    public const string AccountNameKey = "accountName";

    /// <summary>
    /// The key of the Secret that holds the account key of Azure Blob.
    /// </summary>
    public const string AccountKeyKey = "accountKey";

    /// <summary>
    /// Gets or sets the provider: <c>S3</c>, <c>AzureBlob</c> or <c>Volume</c>.
    /// </summary>
    [Description("S3 (the default; UpCloud, AWS, Google Cloud Storage in S3 mode, MinIO, Cloudflare R2 and others), AzureBlob or Volume (development only).")]
    public string Provider { get; set; } = ObjectStorageProviders.S3;

    /// <summary>
    /// Gets or sets the endpoint of an S3-compatible provider.
    /// </summary>
    [Description("S3: the service endpoint, such as https://example.upcloudobjects.com. Empty means AWS.")]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the region of an S3-compatible provider.
    /// </summary>
    [Description("S3: the region.")]
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bucket or container.
    /// </summary>
    [Description("The bucket (S3) or container (AzureBlob).")]
    public string Bucket { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the bucket is addressed in the path rather than the host name.
    /// </summary>
    [Description("S3: address the bucket in the path instead of the host name. MinIO and most self-hosted providers need it.")]
    public bool PathStyle { get; set; }

    /// <summary>
    /// Gets or sets the service URL of Azure Blob.
    /// </summary>
    [Description("AzureBlob: the service URL, such as an Azurite endpoint. Empty means the public cloud endpoint of the account.")]
    public string BlobServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the PersistentVolumeClaim a Volume provider writes to.
    /// </summary>
    [Description("Volume: the name of the PersistentVolumeClaim the backups are written to.")]
    public string VolumeClaimName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the Secret that holds the credentials.
    /// </summary>
    [Description("The Secret that holds the credentials: accessKey and secretKey for S3, accountName and accountKey for AzureBlob. Not used by Volume.")]
    public string CredentialsSecretName { get; set; } = string.Empty;
}
