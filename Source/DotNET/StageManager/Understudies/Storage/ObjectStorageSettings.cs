// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// Everything needed to reach one bucket or container. It carries credentials, so it is never logged or serialized into a status; it has no printable form on purpose.
/// </summary>
public sealed record ObjectStorageSettings
{
    /// <summary>
    /// Gets the provider kind, one of <see cref="ObjectStorageProviders"/>.
    /// </summary>
    public string Provider { get; init; } = ObjectStorageProviders.S3;

    /// <summary>
    /// Gets the service endpoint of an S3-compatible provider, such as <c>https://example.upcloudobjects.com</c>. Empty means AWS.
    /// </summary>
    public string Endpoint { get; init; } = string.Empty;

    /// <summary>
    /// Gets the region of an S3-compatible provider.
    /// </summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>
    /// Gets the bucket (S3) or container (Azure Blob).
    /// </summary>
    public string Bucket { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether the bucket is addressed in the path instead of the host name, which MinIO and most self-hosted providers need.
    /// </summary>
    public bool PathStyle { get; init; }

    /// <summary>
    /// Gets the access key of an S3-compatible provider.
    /// </summary>
    public string AccessKey { get; init; } = string.Empty;

    /// <summary>
    /// Gets the secret key of an S3-compatible provider.
    /// </summary>
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>
    /// Gets the storage account of Azure Blob.
    /// </summary>
    public string AccountName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the account key of Azure Blob.
    /// </summary>
    public string AccountKey { get; init; } = string.Empty;

    /// <summary>
    /// Gets the service URL of Azure Blob, such as an Azurite endpoint. Empty means the public cloud endpoint of the account.
    /// </summary>
    public string BlobServiceUrl { get; init; } = string.Empty;

    /// <summary>
    /// Gets the directory a Volume provider writes to.
    /// </summary>
    public string VolumePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets a description that holds none of the credentials a record would print.
    /// </summary>
    /// <returns>The description.</returns>
    public override string ToString() => $"{nameof(ObjectStorageSettings)} ({Provider})";
}
