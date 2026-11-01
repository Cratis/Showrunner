// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// Defines how an application sends email.
/// </summary>
public interface IEmails
{
    /// <summary>
    /// Sends an email: renders it to make sure it can be sent, then queues it. Delivery happens in the background, with
    /// retries, and ends with <see cref="EmailSent"/> or <see cref="EmailFailed"/> recorded against
    /// <see cref="EmailRequest.About"/>.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the sending.</param>
    /// <returns>The identifier of the email, which every fact about it carries.</returns>
    /// <exception cref="Rendering.EmailCannotBeRendered">The email has an unknown or missing value, or its template or an image is not valid. Nothing is queued.</exception>
    /// <exception cref="EmailNotQueued">The event log did not take the email.</exception>
    Task<EmailId> Send(EmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records that the receiving side reported that an email did not reach the recipient.
    /// </summary>
    /// <param name="about">Whatever the email was about.</param>
    /// <param name="emailId">The email.</param>
    /// <param name="kind">The kind of email.</param>
    /// <param name="reason">What was reported, without the recipient's address.</param>
    /// <param name="cancellationToken">Cancels the recording.</param>
    /// <returns>An awaitable task.</returns>
    Task Bounced(EventSourceId about, EmailId emailId, EmailKind kind, FailureReason reason, CancellationToken cancellationToken = default);
}
