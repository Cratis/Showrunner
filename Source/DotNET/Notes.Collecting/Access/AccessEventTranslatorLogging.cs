// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Collecting.Access;

/// <summary>
/// Log messages for <see cref="AccessEventTranslator"/>.
/// </summary>
internal static partial class AccessEventTranslatorLogging
{
    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' from access store '{AccessStore}': it is not an event Notes receives about access")]
    internal static partial void NotAnAccessEvent(this ILogger<AccessEventTranslator> logger, string eventType, EventStoreName accessStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' from access store '{AccessStore}': it names no tenant")]
    internal static partial void NoTenant(this ILogger<AccessEventTranslator> logger, string eventType, EventStoreName accessStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' from access store '{AccessStore}': it names no user")]
    internal static partial void NoUser(this ILogger<AccessEventTranslator> logger, string eventType, EventStoreName accessStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' from access store '{AccessStore}': there is no tenant namespace '{TenantId}'")]
    internal static partial void UnknownTenant(this ILogger<AccessEventTranslator> logger, string eventType, TenantId tenantId, EventStoreName accessStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' from access store '{AccessStore}': more than one namespace is named '{TenantId}', so the tenant can not be told")]
    internal static partial void AmbiguousTenant(this ILogger<AccessEventTranslator> logger, string eventType, TenantId tenantId, EventStoreName accessStore);

    [LoggerMessage(LogLevel.Debug, "'{EventType}' for '{GiverId}' in tenant '{TenantId}' from access store '{AccessStore}' was already recorded")]
    internal static partial void AlreadyRecorded(this ILogger<AccessEventTranslator> logger, string eventType, EventSourceId giverId, TenantId tenantId, EventStoreName accessStore);
}
