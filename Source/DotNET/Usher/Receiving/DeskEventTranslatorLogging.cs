// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Receiving;

/// <summary>
/// Log messages for <see cref="DeskEventTranslator"/>.
/// </summary>
internal static partial class DeskEventTranslatorLogging
{
    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' from desk store '{DeskStore}': it is not an event Usher receives from a desk")]
    internal static partial void NotAnEventFromTheDesk(this ILogger<DeskEventTranslator> logger, string eventType, EventStoreName deskStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for issue '{IssueId}' from desk store '{DeskStore}': it names no tenant")]
    internal static partial void NoTenant(this ILogger<DeskEventTranslator> logger, string eventType, EventSourceId issueId, EventStoreName deskStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for issue '{IssueId}' from desk store '{DeskStore}': there is no tenant namespace '{TenantId}'")]
    internal static partial void UnknownTenant(this ILogger<DeskEventTranslator> logger, string eventType, EventSourceId issueId, TenantId tenantId, EventStoreName deskStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for issue '{IssueId}' from desk store '{DeskStore}': more than one namespace is named '{TenantId}', so the tenant can not be told")]
    internal static partial void AmbiguousTenant(this ILogger<DeskEventTranslator> logger, string eventType, EventSourceId issueId, TenantId tenantId, EventStoreName deskStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for issue '{IssueId}' from desk store '{DeskStore}': no such issue was reported in tenant '{TenantId}'")]
    internal static partial void UnknownIssue(this ILogger<DeskEventTranslator> logger, string eventType, EventSourceId issueId, TenantId tenantId, EventStoreName deskStore);

    [LoggerMessage(LogLevel.Debug, "'{EventType}' for issue '{IssueId}' in tenant '{TenantId}' from desk store '{DeskStore}' was already recorded")]
    internal static partial void AlreadyRecorded(this ILogger<DeskEventTranslator> logger, string eventType, EventSourceId issueId, TenantId tenantId, EventStoreName deskStore);
}
