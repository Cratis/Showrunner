// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Transport;

/// <summary>
/// Defines how a message leaves the application. Herald ships SMTP and a folder for development; register another
/// implementation before calling <c>AddHerald</c> to send through a provider.
/// </summary>
public interface IEmailTransport
{
    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="cancellationToken">Cancels the sending.</param>
    /// <returns>An awaitable task that completes when the message was accepted for delivery.</returns>
    /// <exception cref="EmailTransportException">The message was not accepted. Any other exception is treated as a failure that may be retried.</exception>
    Task Send(EmailMessage message, CancellationToken cancellationToken = default);
}
