// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// Recorded when the mail server did not take an email and Herald will try again.
/// </summary>
/// <param name="EmailId">The email.</param>
/// <param name="Attempt">How many deliveries have failed so far.</param>
/// <param name="Reason">Why this one failed.</param>
/// <param name="NextAttemptAt">When Herald tries again.</param>
[EventType]
public record EmailAttemptFailed(EmailId EmailId, int Attempt, FailureReason Reason, DateTimeOffset NextAttemptAt);
