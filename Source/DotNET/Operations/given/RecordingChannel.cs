// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An <see cref="IAlertChannel"/> that records what it was asked to deliver, and fails on demand.
/// </summary>
/// <param name="name">The name of the channel.</param>
public sealed class RecordingChannel(string name = "recording") : IAlertChannel
{
    /// <summary>
    /// Gets the notifications delivered.
    /// </summary>
    public List<AlertNotification> Delivered { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether delivery throws, as a transport error carrying a secret would.
    /// </summary>
    public bool Fail { get; set; }

    /// <inheritdoc/>
    public string Name => name;

    /// <inheritdoc/>
    public Task Deliver(AlertNotification notification, CancellationToken cancellationToken)
    {
        if (Fail)
        {
            throw new HttpRequestException("POST https://hooks.example.com/secret-token failed");
        }

        Delivered.Add(notification);
        return Task.CompletedTask;
    }
}
#endif
