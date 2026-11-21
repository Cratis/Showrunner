// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Alerts;

/// <summary>
/// Delivers a notification to every configured channel. One failing channel never silences the rest; whether a partial failure
/// fails the run is for the caller to decide from the <see cref="DeliveryResult"/>.
/// </summary>
/// <param name="channels">The configured channels.</param>
/// <param name="logger">The logger.</param>
public sealed class AlertDelivery(IEnumerable<IAlertChannel> channels, ILogger<AlertDelivery> logger)
{
    readonly IReadOnlyList<IAlertChannel> _channels = [.. channels];

    /// <summary>
    /// Delivers a notification.
    /// </summary>
    /// <param name="notification">What to tell.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The channels that delivered and the ones that failed.</returns>
    public async Task<DeliveryResult> Deliver(AlertNotification notification, CancellationToken cancellationToken)
    {
        var delivered = new List<string>();
        var failed = new List<string>();
        foreach (var channel in _channels)
        {
            try
            {
                await channel.Deliver(notification, cancellationToken);
                delivered.Add(channel.Name);
                logger.Delivered(channel.Name);
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                // The exception is deliberately not logged: transport errors can contain capability URLs and credentials.
                failed.Add(channel.Name);
                logger.DeliveryFailed(channel.Name);
            }
        }

        if (_channels.Count == 0)
        {
            logger.NoChannels();
        }

        return new DeliveryResult(delivered, failed);
    }
}
