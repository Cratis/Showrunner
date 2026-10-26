// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Log messages for <see cref="OnBehalfSessionCalls"/>. They never carry a token, and never the people involved.
/// </summary>
internal static partial class OnBehalfSessionCallsLogging
{
    [LoggerMessage(LogLevel.Warning, "Refused an on-behalf call for {Purpose}: its token was not accepted ({Rejection})")]
    internal static partial void TokenRejected(this ILogger<OnBehalfSessionCalls> logger, string purpose, OnBehalfTokenRejection rejection);

    [LoggerMessage(LogLevel.Warning, "Refused an on-behalf call for {Purpose}: the tenant header disagrees with the token")]
    internal static partial void TenantHeaderDisagrees(this ILogger<OnBehalfSessionCalls> logger, string purpose);

    [LoggerMessage(LogLevel.Warning, "Refused an on-behalf notification for {Purpose}: its body is not about the session its token is for")]
    internal static partial void NotificationNotAboutTheSession(this ILogger<OnBehalfSessionCalls> logger, string purpose);

    [LoggerMessage(LogLevel.Information, "An on-behalf call named a tenant or an issue Usher does not know")]
    internal static partial void UnknownTenantOrIssue(this ILogger<OnBehalfSessionCalls> logger);

    [LoggerMessage(LogLevel.Error, "The represented principal for issue {IssueId} names somebody other than the reporter; the grant is answered not live")]
    internal static partial void PrincipalForSomebodyElse(this ILogger<OnBehalfSessionCalls> logger, IssueId issueId);
}
