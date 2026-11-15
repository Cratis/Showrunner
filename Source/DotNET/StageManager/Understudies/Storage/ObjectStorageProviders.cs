// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// The kinds of object storage an Understudy can write to.
/// </summary>
public static class ObjectStorageProviders
{
    /// <summary>
    /// Anything that speaks the S3 API: UpCloud Object Storage, AWS, Google Cloud Storage in its S3 mode, MinIO, Cloudflare R2 and others. The default.
    /// </summary>
    public const string S3 = "S3";

    /// <summary>
    /// Azure Blob Storage, through its own client.
    /// </summary>
    public const string AzureBlob = "AzureBlob";

    /// <summary>
    /// A volume, for local and development use. Downloads stream through the operator.
    /// </summary>
    public const string Volume = "Volume";

    /// <summary>
    /// Gets whether the kind is one of the supported ones.
    /// </summary>
    /// <param name="provider">The kind.</param>
    /// <returns>True when it is supported.</returns>
    public static bool IsSupported(string provider) =>
        string.Equals(provider, S3, StringComparison.Ordinal) ||
        string.Equals(provider, AzureBlob, StringComparison.Ordinal) ||
        string.Equals(provider, Volume, StringComparison.Ordinal);
}
