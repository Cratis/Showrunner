// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Contracts.Emails;

/// <summary>
/// Recorded when the mail server accepted an email for delivery. It is recorded against whatever the email was about,
/// for example the invitation that was sent. The recipient is not part of the fact: the host already knows who the
/// invitation was for.
/// </summary>
/// <param name="EmailId">The email.</param>
/// <param name="Kind">The kind of email, which is its template.</param>
/// <param name="Language">The language it was written in.</param>
/// <param name="Attempts">How many deliveries it took, at least one.</param>
[EventType]
public record EmailSent(EmailId EmailId, EmailKind Kind, EmailLanguage Language, int Attempts);
