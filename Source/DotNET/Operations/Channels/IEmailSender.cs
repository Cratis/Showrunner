// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Channels;

/// <summary>
/// Sends an email.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the server accepted the email. It throws when it did not.</returns>
    Task Send(OutgoingEmail email, CancellationToken cancellationToken);
}
