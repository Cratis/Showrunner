// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Runtime.CompilerServices;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.given;

/// <summary>
/// Object storage in memory that records what was written and in which order, and that behaves as a bucket with the rules it is given.
/// </summary>
/// <param name="time">The clock that stamps what is written.</param>
public class InMemoryObjectStorage(TimeProvider? time = null) : IObjectStorage
{
    readonly Dictionary<string, (byte[] Content, DateTimeOffset LastModified)> _objects = new(StringComparer.Ordinal);

    public TimeProvider Time { get; set; } = time ?? TimeProvider.System;

    public string Provider => ObjectStorageProviders.S3;

    public bool SupportsSignedLinks => true;

    public bool? Versioned { get; set; } = false;

    public bool? ObjectLocked { get; set; } = false;

    public bool CanEnforceExpiry { get; set; } = true;

    public bool ReadBackExpiry { get; set; } = true;

    public bool Unreachable { get; set; }

    public int? ExpiryDays { get; private set; }

    public List<string> Writes { get; } = [];

    public List<string> Deletes { get; } = [];

    public IEnumerable<string> Keys => _objects.Keys.Order(StringComparer.Ordinal);

    public byte[] ContentOf(string key) => _objects[key].Content;

    public bool Has(string key) => _objects.ContainsKey(key);

    public void Seed(string key, byte[] content, DateTimeOffset? lastModified = null) => _objects[key] = (content, lastModified ?? Time.GetUtcNow());

    public async Task PutAsync(string key, Stream content, CancellationToken cancellationToken)
    {
        ThrowWhenUnreachable();
        await using var copy = new MemoryStream();
        await content.CopyToAsync(copy, cancellationToken);
        _objects[key] = (copy.ToArray(), Time.GetUtcNow());
        Writes.Add(key);
    }

    public Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        ThrowWhenUnreachable();
        return Task.FromResult<Stream?>(_objects.TryGetValue(key, out var found) ? new MemoryStream(found.Content) : null);
    }

    public async IAsyncEnumerable<StoredObject> ListAsync(string prefix, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ThrowWhenUnreachable();
        await Task.CompletedTask;
        foreach (var (key, value) in _objects.Where(pair => pair.Key.StartsWith(prefix, StringComparison.Ordinal)).OrderBy(pair => pair.Key, StringComparer.Ordinal).ToList())
        {
            yield return new(key, value.Content.Length, value.LastModified);
        }
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        _objects.Remove(key);
        Deletes.Add(key);
        return Task.CompletedTask;
    }

    public Task<DownloadLink> CreateDownloadLinkAsync(string key, TimeSpan? validFor, CancellationToken cancellationToken) =>
        Task.FromResult(new DownloadLink(new Uri($"https://bucket.example/{key}"), Time.GetUtcNow().Add(validFor ?? DownloadLink.DefaultValidity)));

    public Task<BucketState> InspectAsync(CancellationToken cancellationToken)
    {
        ThrowWhenUnreachable();
        return Task.FromResult(new BucketState(Versioned, ObjectLocked, ReadBackExpiry ? ExpiryDays : null));
    }

    public Task<bool> TryEnforceExpiryAsync(int days, CancellationToken cancellationToken)
    {
        ThrowWhenUnreachable();
        if (!CanEnforceExpiry)
        {
            return Task.FromResult(false);
        }

        ExpiryDays = days;
        return Task.FromResult(true);
    }

    void ThrowWhenUnreachable()
    {
        if (Unreachable)
        {
            throw new HttpRequestException("unreachable");
        }
    }
}
#endif
