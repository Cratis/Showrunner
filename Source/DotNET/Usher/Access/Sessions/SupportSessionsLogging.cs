// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// Log messages for <see cref="SupportSessions"/>. They name issues and namespaces, never the people involved.
/// </summary>
internal static partial class SupportSessionsLogging
{
    [LoggerMessage(LogLevel.Information, "Support may not act as the reporter on issue {IssueId} in namespace {Namespace}: {Refusal}")]
    internal static partial void SessionRefused(this ILogger<SupportSessions> logger, IssueId issueId, SupportSessionRefusal refusal, EventStoreNamespaceName @namespace);

    [LoggerMessage(LogLevel.Information, "Support signed in as the reporter on issue {IssueId} in namespace {Namespace}")]
    internal static partial void SessionStarted(this ILogger<SupportSessions> logger, IssueId issueId, EventStoreNamespaceName @namespace);

    [LoggerMessage(LogLevel.Information, "A session in which support signed in as the reporter on issue {IssueId} in namespace {Namespace} ended")]
    internal static partial void SessionEnded(this ILogger<SupportSessions> logger, IssueId issueId, EventStoreNamespaceName @namespace);

    [LoggerMessage(LogLevel.Debug, "The access on issue {IssueId} in namespace {Namespace} changed while a session was starting; deciding again")]
    internal static partial void AccessChangedWhileStarting(this ILogger<SupportSessions> logger, IssueId issueId, EventStoreNamespaceName @namespace);

    [LoggerMessage(LogLevel.Warning, "The end of a session that never started on issue {IssueId} in namespace {Namespace} was reported; nothing was recorded")]
    internal static partial void EndOfUnknownSession(this ILogger<SupportSessions> logger, IssueId issueId, EventStoreNamespaceName @namespace);
}
