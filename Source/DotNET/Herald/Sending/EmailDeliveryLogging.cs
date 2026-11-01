// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// Log messages for <see cref="EmailDelivery"/>. None of them carries a recipient, a value or a credential.
/// </summary>
internal static partial class EmailDeliveryLogging
{
    [LoggerMessage(LogLevel.Information, "Email {EmailId} ({Kind}) was sent after {Attempts} attempt(s)")]
    internal static partial void Sent(this ILogger<EmailDelivery> logger, EmailId emailId, EmailKind kind, int attempts);

    [LoggerMessage(LogLevel.Warning, "Email {EmailId} ({Kind}) could not be delivered in attempt {Attempt}; trying again at {NextAttemptAt}: {Reason}")]
    internal static partial void WillRetry(this ILogger<EmailDelivery> logger, EmailId emailId, EmailKind kind, int attempt, DateTimeOffset nextAttemptAt, string reason);

    [LoggerMessage(LogLevel.Error, "Email {EmailId} ({Kind}) failed for good after {Attempts} attempt(s): {Reason}")]
    internal static partial void Failed(this ILogger<EmailDelivery> logger, EmailId emailId, EmailKind kind, int attempts, string reason);
}
