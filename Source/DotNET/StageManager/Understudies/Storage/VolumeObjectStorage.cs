// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// <see cref="IObjectStorage"/> over a directory, for local and development use. It has no signed links, so downloads stream through the operator,
/// and no bucket rules, so it cannot hold key backups.
/// </summary>
/// <param name="root">The directory objects are written under.</param>
public class VolumeObjectStorage(string root) : IObjectStorage
{
    readonly string _root = Path.GetFullPath(root);

    /// <inheritdoc/>
    public string Provider => ObjectStorageProviders.Volume;

    /// <inheritdoc/>
    public bool SupportsSignedLinks => false;

    /// <inheritdoc/>
    public async Task PutAsync(string key, Stream content, CancellationToken cancellationToken)
    {
        var path = PathOf(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        var path = PathOf(key);
        return Task.FromResult<Stream?>(File.Exists(path) ? File.OpenRead(path) : null);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<StoredObject> ListAsync(string prefix, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        if (!Directory.Exists(_root))
        {
            yield break;
        }

        foreach (var path in Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var key = Path.GetRelativePath(_root, path).Replace(Path.DirectorySeparatorChar, '/');
            if (key.StartsWith(prefix, StringComparison.Ordinal))
            {
                var info = new FileInfo(path);
                yield return new(key, info.Length, new DateTimeOffset(info.LastWriteTimeUtc, TimeSpan.Zero));
            }
        }
    }

    /// <inheritdoc/>
    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        var path = PathOf(key);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<DownloadLink> CreateDownloadLinkAsync(string key, TimeSpan? validFor, CancellationToken cancellationToken) =>
        throw new NotSupportedException("A volume has no signed links. Stream the download through the operator.");

    /// <inheritdoc/>
    public Task<BucketState> InspectAsync(CancellationToken cancellationToken) => Task.FromResult(BucketState.Unknown);

    /// <inheritdoc/>
    public Task<bool> TryEnforceExpiryAsync(int days, CancellationToken cancellationToken) => Task.FromResult(false);

    string PathOf(string key)
    {
        var path = Path.GetFullPath(Path.Combine(_root, key));
        if (!path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new ArgumentException("The key leaves the volume.", nameof(key));
        }

        return path;
    }
}
