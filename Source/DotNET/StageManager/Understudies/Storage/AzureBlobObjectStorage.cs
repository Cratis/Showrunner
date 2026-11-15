// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// <see cref="IObjectStorage"/> over Azure Blob Storage, with shared access signatures for downloads. Versioning, immutability and lifecycle rules of a storage account
/// are management-plane settings that the blob client cannot read, so this provider cannot prove the rules a key bucket needs and refuses key backups.
/// </summary>
public class AzureBlobObjectStorage : IObjectStorage
{
    readonly BlobContainerClient _container;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobObjectStorage"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public AzureBlobObjectStorage(ObjectStorageSettings settings)
    {
        var service = string.IsNullOrEmpty(settings.BlobServiceUrl)
            ? new Uri($"https://{settings.AccountName}.blob.core.windows.net")
            : new Uri(settings.BlobServiceUrl);
        var credential = new StorageSharedKeyCredential(settings.AccountName, settings.AccountKey);
        _container = new BlobServiceClient(service, credential).GetBlobContainerClient(settings.Bucket);
    }

    /// <inheritdoc/>
    public string Provider => ObjectStorageProviders.AzureBlob;

    /// <inheritdoc/>
    public bool SupportsSignedLinks => true;

    /// <inheritdoc/>
    public async Task PutAsync(string key, Stream content, CancellationToken cancellationToken)
    {
        await _container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        await _container.GetBlobClient(key).UploadAsync(content, overwrite: true, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            return await _container.GetBlobClient(key).OpenReadAsync(cancellationToken: cancellationToken);
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<StoredObject> ListAsync(string prefix, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!await _container.ExistsAsync(cancellationToken))
        {
            yield break;
        }

        await foreach (var blob in _container.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, cancellationToken))
        {
            yield return new(blob.Name, blob.Properties.ContentLength ?? 0, blob.Properties.LastModified ?? DateTimeOffset.UnixEpoch);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string key, CancellationToken cancellationToken) =>
        await _container.GetBlobClient(key).DeleteIfExistsAsync(cancellationToken: cancellationToken);

    /// <inheritdoc/>
    public Task<DownloadLink> CreateDownloadLinkAsync(string key, TimeSpan? validFor, CancellationToken cancellationToken)
    {
        var expires = DateTimeOffset.UtcNow.Add(validFor ?? DownloadLink.DefaultValidity);
        var url = _container.GetBlobClient(key).GenerateSasUri(BlobSasPermissions.Read, expires);
        return Task.FromResult(new DownloadLink(url, expires));
    }

    /// <inheritdoc/>
    public Task<BucketState> InspectAsync(CancellationToken cancellationToken) => Task.FromResult(BucketState.Unknown);

    /// <inheritdoc/>
    public Task<bool> TryEnforceExpiryAsync(int days, CancellationToken cancellationToken) => Task.FromResult(false);
}
