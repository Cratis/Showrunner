// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Access;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport;

/// <summary>
/// Every <see cref="NotesOptions.AccessRefreshInterval"/> asks the collector which users the desk granted the right to give
/// feedback and keeps the answer in the <see cref="GrantedGiversCache"/>. It does nothing unless
/// <see cref="NotesOptions.AccessMode"/> is <see cref="FeedbackAccessMode.GrantedUsers"/> and
/// <see cref="NotesOptions.Transport"/> is <see cref="NotesTransport.Http"/>.
/// </summary>
/// <remarks>
/// A refresh that fails is logged and tried again at the next interval; the cache keeps the last list for as long as
/// <see cref="NotesOptions.AccessMaxStaleness"/> allows. The host keeps running.
/// </remarks>
/// <param name="cache">The cache of granted users.</param>
/// <param name="options">Notes' options.</param>
/// <param name="time">The clock.</param>
/// <param name="logger">The logger.</param>
public class GrantedAccessWorker(
    GrantedGiversCache cache,
    IOptions<NotesOptions> options,
    TimeProvider time,
    ILogger<GrantedAccessWorker> logger) : BackgroundService
{
    /// <summary>
    /// Gets whether the application takes the granted users from the collector.
    /// </summary>
    /// <param name="options">Notes' options.</param>
    /// <returns><see langword="true"/> when it does.</returns>
    public static bool IsNeeded(NotesOptions options) =>
        options.Enabled && options.Transport == NotesTransport.Http && options.AccessMode == FeedbackAccessMode.GrantedUsers;

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!IsNeeded(options.Value))
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await cache.Refresh(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.AccessRefreshFailed(exception, (int)options.Value.AccessRefreshInterval.TotalSeconds);
            }

            try
            {
                await Task.Delay(options.Value.AccessRefreshInterval, time, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
