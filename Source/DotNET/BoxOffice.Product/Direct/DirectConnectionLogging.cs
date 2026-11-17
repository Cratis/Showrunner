// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

internal static partial class DirectConnectionLogging
{
    [LoggerMessage(LogLevel.Information, "An enrollment code was redeemed and its credentials stored")]
    internal static partial void Redeemed(this ILogger<DirectConnection> logger);

    [LoggerMessage(LogLevel.Warning, "An enrollment code was not redeemed: {Outcome}")]
    internal static partial void NotRedeemed(this ILogger<DirectConnection> logger, DirectRedemption outcome);

    [LoggerMessage(LogLevel.Warning, "The link to Chronicle is stored but the first exchange failed; the next attempt retries with the stored credentials")]
    internal static partial void ConnectionFailed(this ILogger<DirectConnection> logger);

    [LoggerMessage(LogLevel.Information, "The link to Chronicle is established and verified; the setup page is closed")]
    internal static partial void Connected(this ILogger<DirectConnection> logger);

    [LoggerMessage(LogLevel.Warning, "An endpoint that is not on the pinned host was refused")]
    internal static partial void EndpointRefused(this ILogger<DirectConnection> logger);

    [LoggerMessage(LogLevel.Warning, "Too many attempts to link the product to Chronicle; none is made until the window passes")]
    internal static partial void TooManyAttempts(this ILogger<DirectConnection> logger);

    [LoggerMessage(LogLevel.Information, "Redeeming the enrollment code from configuration came to {Result}")]
    internal static partial void ConfiguredAttempt(this ILogger<DirectEnrollmentWorker> logger, DirectAttemptResult result);

    [LoggerMessage(LogLevel.Error, "Redeeming the enrollment code from configuration failed: {ExceptionType}")]
    internal static partial void ConfiguredAttemptFailed(this ILogger<DirectEnrollmentWorker> logger, string exceptionType);

    [LoggerMessage(LogLevel.Warning, "A post to the setup page was refused because {Reason}; nothing was redeemed")]
    internal static partial void PostRefused(this ILogger<DirectConnectionCalls> logger, string reason);

    [LoggerMessage(LogLevel.Warning, "Someone who is not a configured administrator asked for the setup page")]
    internal static partial void NotAnAdministrator(this ILogger<DirectConnectionCalls> logger);

    [LoggerMessage(LogLevel.Warning, "The link to Chronicle can never work again, so the setup page opened again")]
    internal static partial void Reopened(this ILogger<DirectConnection> logger);
}
