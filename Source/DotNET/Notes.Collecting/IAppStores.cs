// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Collecting;

/// <summary>
/// Defines which application event stores the collecting side reads notes from. A builder of applications publishes new
/// ones while it runs, so the host can supply its own to name them as they appear; every round of the background work
/// asks again.
/// </summary>
public interface IAppStores
{
    /// <summary>
    /// Gets the names of the application event stores to read notes from.
    /// </summary>
    /// <param name="cancellationToken">Stops waiting when the host shuts down.</param>
    /// <returns>The event store names.</returns>
    Task<IEnumerable<EventStoreName>> Get(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an implementation of <see cref="IAppStores"/> that names the stores in <see cref="NoteCollectingOptions.AppStores"/>.
/// </summary>
/// <param name="options">The <see cref="NoteCollectingOptions"/>.</param>
public class ConfiguredAppStores(IOptions<NoteCollectingOptions> options) : IAppStores
{
    /// <inheritdoc/>
    public Task<IEnumerable<EventStoreName>> Get(CancellationToken cancellationToken = default) =>
        Task.FromResult(options.Value.AppStores);
}
