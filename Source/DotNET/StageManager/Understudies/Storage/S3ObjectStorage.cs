// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Runtime.CompilerServices;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// <see cref="IObjectStorage"/> over the S3 API: UpCloud Object Storage, AWS, Google Cloud Storage in its S3 mode, MinIO, Cloudflare R2 and others.
/// </summary>
public sealed class S3ObjectStorage : IObjectStorage, IDisposable
{
    /// <summary>
    /// The id of the lifecycle rule the Understudy owns. Other rules of the bucket are left alone.
    /// </summary>
    public const string ExpiryRuleId = "showrunner-understudy-expiry";

    readonly AmazonS3Client _client;
    readonly string _bucket;

    /// <summary>
    /// Initializes a new instance of the <see cref="S3ObjectStorage"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public S3ObjectStorage(ObjectStorageSettings settings)
    {
        _bucket = settings.Bucket;
        var config = new AmazonS3Config
        {
            ForcePathStyle = settings.PathStyle,

            // Third-party providers do not implement the additional checksums newer SDK versions send by default.
            RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
            ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
        };

        if (!string.IsNullOrEmpty(settings.Endpoint))
        {
            config.ServiceURL = settings.Endpoint;
            config.AuthenticationRegion = string.IsNullOrEmpty(settings.Region) ? "us-east-1" : settings.Region;
        }
        else if (!string.IsNullOrEmpty(settings.Region))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(settings.Region);
        }

        _client = new(new BasicAWSCredentials(settings.AccessKey, settings.SecretKey), config);
    }

    /// <inheritdoc/>
    public string Provider => ObjectStorageProviders.S3;

    /// <inheritdoc/>
    public bool SupportsSignedLinks => true;

    /// <inheritdoc/>
    public async Task PutAsync(string key, Stream content, CancellationToken cancellationToken) =>
        await _client.PutObjectAsync(new PutObjectRequest { BucketName = _bucket, Key = key, InputStream = content, AutoCloseStream = false }, cancellationToken);

    /// <inheritdoc/>
    public async Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.GetObjectAsync(_bucket, key, cancellationToken);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<StoredObject> ListAsync(string prefix, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string? token = null;
        do
        {
            var response = await _client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = _bucket, Prefix = prefix, ContinuationToken = token }, cancellationToken);
            foreach (var item in response.S3Objects ?? [])
            {
                yield return new(item.Key, item.Size ?? 0, new DateTimeOffset(DateTime.SpecifyKind(item.LastModified ?? DateTime.UnixEpoch, DateTimeKind.Utc)));
            }

            token = response.IsTruncated == true ? response.NextContinuationToken : null;
        }
        while (token is not null);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string key, CancellationToken cancellationToken) =>
        await _client.DeleteObjectAsync(_bucket, key, cancellationToken);

    /// <inheritdoc/>
    public async Task<DownloadLink> CreateDownloadLinkAsync(string key, TimeSpan? validFor, CancellationToken cancellationToken)
    {
        var expires = DateTime.UtcNow.Add(validFor ?? DownloadLink.DefaultValidity);
        var url = await _client.GetPreSignedURLAsync(new GetPreSignedUrlRequest { BucketName = _bucket, Key = key, Verb = HttpVerb.GET, Expires = expires });
        return new(new Uri(url), new DateTimeOffset(expires, TimeSpan.Zero));
    }

    /// <inheritdoc/>
    public async Task<BucketState> InspectAsync(CancellationToken cancellationToken)
    {
        var versioning = await _client.GetBucketVersioningAsync(_bucket, cancellationToken);
        var versioned = versioning.VersioningConfig?.Status == VersionStatus.Enabled || versioning.VersioningConfig?.Status == VersionStatus.Suspended;
        return new(versioned, await IsObjectLockOnAsync(cancellationToken), (await ReadRulesAsync(cancellationToken)).FirstOrDefault(rule => rule.Id == ExpiryRuleId)?.Expiration?.Days);
    }

    /// <inheritdoc/>
    public async Task<bool> TryEnforceExpiryAsync(int days, CancellationToken cancellationToken)
    {
        var rules = (await ReadRulesAsync(cancellationToken)).Where(rule => rule.Id != ExpiryRuleId).ToList();
        rules.Add(new LifecycleRule
        {
            Id = ExpiryRuleId,
            Status = LifecycleRuleStatus.Enabled,
            Filter = new LifecycleFilter { LifecycleFilterPredicate = new LifecyclePrefixPredicate { Prefix = string.Empty } },
            Expiration = new LifecycleRuleExpiration { Days = days }
        });

        await _client.PutLifecycleConfigurationAsync(new PutLifecycleConfigurationRequest { BucketName = _bucket, Configuration = new LifecycleConfiguration { Rules = rules } }, cancellationToken);
        return true;
    }

    /// <inheritdoc/>
    public void Dispose() => _client.Dispose();

    async Task<bool> IsObjectLockOnAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.GetObjectLockConfigurationAsync(new GetObjectLockConfigurationRequest { BucketName = _bucket }, cancellationToken);
            return response.ObjectLockConfiguration?.ObjectLockEnabled == ObjectLockEnabled.Enabled;
        }
        catch (AmazonS3Exception exception) when (exception.ErrorCode == "ObjectLockConfigurationNotFoundError")
        {
            return false;
        }
    }

    async Task<List<LifecycleRule>> ReadRulesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.GetLifecycleConfigurationAsync(_bucket, cancellationToken);
            return response.Configuration?.Rules ?? [];
        }
        catch (AmazonS3Exception exception) when (exception.ErrorCode == "NoSuchLifecycleConfiguration")
        {
            return [];
        }
    }
}
