// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Alerts;

/// <summary>
/// A place alerts are delivered to. A channel throws when delivery fails; <see cref="AlertDelivery"/> makes sure one failing
/// channel does not silence the others.
/// </summary>
public interface IAlertChannel
{
    /// <summary>
    /// Gets the short name of the channel, used in logs. It never contains an address or a credential.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Delivers a notification.
    /// </summary>
    /// <param name="notification">What to tell.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the notification is delivered.</returns>
    Task Deliver(AlertNotification notification, CancellationToken cancellationToken);
}
