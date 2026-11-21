// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Understudies;

/// <summary>
/// The kind of storage an Understudy writes backups to.
/// </summary>
public enum ObjectStorageProvider
{
    /// <summary>
    /// S3 compatible object storage: UpCloud, AWS, Google Cloud Storage in S3 mode, MinIO, Cloudflare R2 and others.
    /// </summary>
    S3 = 0,

    /// <summary>
    /// Azure Blob storage.
    /// </summary>
    AzureBlob = 1,

    /// <summary>
    /// A PersistentVolumeClaim. For development only.
    /// </summary>
    Volume = 2
}
