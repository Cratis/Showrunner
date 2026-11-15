// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// <see cref="IObjectStorageFactory"/> for the supported providers.
/// </summary>
public class ObjectStorageFactory : IObjectStorageFactory
{
    /// <inheritdoc/>
    public IObjectStorage Create(ObjectStorageSettings settings) => settings.Provider switch
    {
        ObjectStorageProviders.S3 => new S3ObjectStorage(settings),
        ObjectStorageProviders.AzureBlob => new AzureBlobObjectStorage(settings),
        ObjectStorageProviders.Volume => new VolumeObjectStorage(settings.VolumePath),
        _ => throw new NotSupportedException($"The object storage provider '{settings.Provider}' is not supported. Use S3, AzureBlob or Volume.")
    };
}
