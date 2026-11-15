// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_ObjectStorageFactory;

public class when_creating_storage : Specification
{
    ObjectStorageFactory _factory;
    IObjectStorage _s3;
    IObjectStorage _azure;
    IObjectStorage _volume;
    Exception? _unsupported;

    void Establish() => _factory = new();

    void Because()
    {
        _s3 = _factory.Create(new() { Provider = ObjectStorageProviders.S3, Endpoint = "https://objects.example", Region = "se-sto1", Bucket = "backups", AccessKey = "a", SecretKey = "b" });
        _azure = _factory.Create(new() { Provider = ObjectStorageProviders.AzureBlob, AccountName = "devstoreaccount1", AccountKey = Convert.ToBase64String(new byte[32]), Bucket = "backups" });
        _volume = _factory.Create(new() { Provider = ObjectStorageProviders.Volume, VolumePath = Path.GetTempPath() });
        _unsupported = Catch.Exception(() => _factory.Create(new() { Provider = "Floppy" }));
    }

    [Fact] void should_create_s3_storage() => _s3.ShouldBeOfExactType<S3ObjectStorage>();
    [Fact] void should_create_azure_blob_storage() => _azure.ShouldBeOfExactType<AzureBlobObjectStorage>();
    [Fact] void should_create_volume_storage() => _volume.ShouldBeOfExactType<VolumeObjectStorage>();
    [Fact] void should_refuse_an_unknown_provider() => _unsupported.ShouldBeOfExactType<NotSupportedException>();
    [Fact] void should_know_the_supported_providers() => new[] { "S3", "AzureBlob", "Volume" }.All(ObjectStorageProviders.IsSupported).ShouldBeTrue();
    [Fact] void should_not_know_others() => ObjectStorageProviders.IsSupported("Floppy").ShouldBeFalse();
}
#endif
