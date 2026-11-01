// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Contracts.Emails;

/// <summary>
/// Recorded when the receiving side reported that an email which the mail server had accepted did not reach the
/// recipient. Herald records it when the host says so, because an SMTP server accepting a message does not know it.
/// </summary>
/// <param name="EmailId">The email.</param>
/// <param name="Kind">The kind of email, which is its template.</param>
/// <param name="Reason">What the receiving side reported.</param>
[EventType]
public record EmailBounced(EmailId EmailId, EmailKind Kind, FailureReason Reason);
