// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// Recorded when Herald accepted an email to send. It is recorded against whatever the email is about, and it is what the
/// delivery works from: the email stays queued until <see cref="EmailSent"/> or <see cref="EmailFailed"/> is recorded.
/// </summary>
/// <param name="EmailId">The email.</param>
/// <param name="Kind">The kind of email, which is its template.</param>
/// <param name="Language">The language asked for; unset for the default language.</param>
/// <param name="Address">Who it is for.</param>
/// <param name="Name">The name shown beside the address.</param>
/// <param name="Values">The values it is filled with.</param>
/// <param name="QueuedAt">When it was accepted.</param>
[EventType]
public record EmailQueued(EmailId EmailId, EmailKind Kind, EmailLanguage Language, RecipientAddress Address, RecipientName Name, EmailValues Values, DateTimeOffset QueuedAt);
