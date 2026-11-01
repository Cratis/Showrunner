// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Contracts.Emails;

/// <summary>
/// Recorded when an email will not be sent: it could not be rendered, the mail server refused it for good, or it was
/// still refused after the last retry. It is recorded against whatever the email was about.
/// </summary>
/// <param name="EmailId">The email.</param>
/// <param name="Kind">The kind of email, which is its template.</param>
/// <param name="Language">The language it was written in.</param>
/// <param name="Reason">Why it will not be sent.</param>
/// <param name="Attempts">How many deliveries were tried; zero when it could not even be rendered.</param>
[EventType]
public record EmailFailed(EmailId EmailId, EmailKind Kind, EmailLanguage Language, FailureReason Reason, int Attempts);
