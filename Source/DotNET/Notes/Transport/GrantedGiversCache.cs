// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Contracts.Access;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport;

/// <summary>
/// Holds the users the desk granted the right to give feedback, as the collector last told the application over HTTP, and
/// answers whether one is granted. It is what the application decides on in place of the facts the same-kernel transport
/// records.
/// </summary>
/// <remarks>
/// <para>
/// It fails closed. Until a list has been fetched nobody is granted. When a refresh fails, the last list stands for
/// <see cref="NotesOptions.AccessMaxStaleness"/> after the last time the collector confirmed it, and beyond that nobody is
/// granted until a refresh succeeds. A refresh that the collector answers with "unchanged" confirms the list as it is now.
/// </para>
/// <para>
/// The list is the publication's: the collector serves one tenant's list to a publication, and every namespace of the
/// application relays under that publication, so the cache is not split by namespace.
/// </para>
/// </remarks>
/// <param name="collector">The collector.</param>
/// <param name="options">Notes' options.</param>
/// <param name="time">The clock.</param>
public class GrantedGiversCache(ICollectorClient collector, IOptions<NotesOptions> options, TimeProvider time)
{
    readonly Lock _lock = new();
    HashSet<Guid>? _givers;
    ulong? _version;
    DateTimeOffset _confirmedAt;

    /// <summary>
    /// Gets whether a list has been fetched.
    /// </summary>
    public bool HasList
    {
        get
        {
            lock (_lock)
            {
                return _givers is not null;
            }
        }
    }

    /// <summary>
    /// Gets the last time the collector confirmed the list it holds.
    /// </summary>
    public DateTimeOffset ConfirmedAt
    {
        get
        {
            lock (_lock)
            {
                return _confirmedAt;
            }
        }
    }

    /// <summary>
    /// Asks the collector for the list and keeps it. A failed call leaves what is held as it was.
    /// </summary>
    /// <param name="cancellationToken">Stops the call when the host shuts down.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="CollectorRequestFailed">The collector refused the request.</exception>
    public async Task Refresh(CancellationToken cancellationToken = default)
    {
        ulong? known;
        lock (_lock)
        {
            known = _givers is null ? null : _version;
        }

        var grants = await collector.GetAccess(known, cancellationToken);
        lock (_lock)
        {
            if (grants is not null)
            {
                _givers = [.. grants.Givers];
                _version = grants.Version;
            }
            else if (_givers is null)
            {
                // The collector said "unchanged" to a request that named no version: there is nothing it could mean.
                return;
            }

            _confirmedAt = time.GetUtcNow();
        }
    }

    /// <summary>
    /// Gets whether a user is granted: a list was fetched, it was confirmed within
    /// <see cref="NotesOptions.AccessMaxStaleness"/> and names the user.
    /// </summary>
    /// <param name="id">The user's right to give feedback.</param>
    /// <returns><see langword="true"/> when the user is granted, otherwise <see langword="false"/>.</returns>
    public bool IsGranted(FeedbackGiverId id)
    {
        lock (_lock)
        {
            if (_givers is null || time.GetUtcNow() - _confirmedAt > options.Value.AccessMaxStaleness)
            {
                return false;
            }

            return _givers.Contains(id.Value);
        }
    }
}
