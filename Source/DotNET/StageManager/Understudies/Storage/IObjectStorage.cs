// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// Where everything an Understudy writes lives, and what the API hands out. One abstraction over S3-compatible storage, Azure Blob and a volume,
/// so backups, the catalog and downloads never know which one they use. Keys are forward-slash paths relative to the bucket or container.
/// </summary>
public interface IObjectStorage
{
    /// <summary>
    /// Gets the provider kind, one of <see cref="ObjectStorageProviders"/>.
    /// </summary>
    string Provider { get; }

    /// <summary>
    /// Gets whether the provider can create short-lived signed links. A provider that cannot has its downloads streamed through the operator.
    /// </summary>
    bool SupportsSignedLinks { get; }

    /// <summary>
    /// Writes an object, replacing one with the same key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="content">The content. It must be seekable, so its length is known.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the object is stored.</returns>
    Task PutAsync(string key, Stream content, CancellationToken cancellationToken);

    /// <summary>
    /// Opens an object for reading.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The content, which the caller disposes, or null when the object does not exist.</returns>
    Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the objects under a prefix, in any depth.
    /// </summary>
    /// <param name="prefix">The prefix, or empty for everything.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The objects.</returns>
    IAsyncEnumerable<StoredObject> ListAsync(string prefix, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an object. Deleting one that does not exist is not an error.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the object is gone.</returns>
    Task DeleteAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a short-lived link a client can download an object from without credentials.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="validFor">How long the link works, or null for <see cref="DownloadLink.DefaultValidity"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The link.</returns>
    /// <exception cref="NotSupportedException">Thrown when <see cref="SupportsSignedLinks"/> is false.</exception>
    Task<DownloadLink> CreateDownloadLinkAsync(string key, TimeSpan? validFor, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the rules the bucket is under. A rule the provider cannot read is reported as unknown, never as holding.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The state.</returns>
    Task<BucketState> InspectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Applies a rule that expires every object after the given number of days, so retention holds even when the operator is down.
    /// </summary>
    /// <param name="days">The days.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True when the rule was applied; false when the provider has no such rule.</returns>
    Task<bool> TryEnforceExpiryAsync(int days, CancellationToken cancellationToken);
}
