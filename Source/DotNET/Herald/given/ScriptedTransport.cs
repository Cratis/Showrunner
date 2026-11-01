// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Transport;

namespace Cratis.Showrunner.Herald.given;

/// <summary>
/// A transport that takes messages or fails, as it is told.
/// </summary>
public class ScriptedTransport : IEmailTransport
{
    /// <summary>
    /// Gets or sets the failure to throw; null to accept the message.
    /// </summary>
    public Exception? Failure { get; set; }

    /// <summary>
    /// Gets the messages that were accepted.
    /// </summary>
    public List<EmailMessage> Accepted { get; } = [];

    /// <summary>
    /// Gets how many times a message was offered.
    /// </summary>
    public int Offered { get; private set; }

    /// <inheritdoc/>
    public Task Send(EmailMessage message, CancellationToken cancellationToken = default)
    {
        Offered++;
        if (Failure is not null)
        {
            return Task.FromException(Failure);
        }

        Accepted.Add(message);
        return Task.CompletedTask;
    }
}
#endif
