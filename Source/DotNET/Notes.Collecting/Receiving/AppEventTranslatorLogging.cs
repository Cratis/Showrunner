// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Collecting.Receiving;

/// <summary>
/// Log messages for <see cref="AppEventTranslator"/>.
/// </summary>
internal static partial class AppEventTranslatorLogging
{
    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' from application store '{AppStore}': it is not an event the collecting side receives")]
    internal static partial void NotAnEventFromAnApplication(this ILogger<AppEventTranslator> logger, string eventType, EventStoreName appStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for note '{NoteId}' from application store '{AppStore}': it names no tenant")]
    internal static partial void NoTenant(this ILogger<AppEventTranslator> logger, string eventType, EventSourceId noteId, EventStoreName appStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for note '{NoteId}' from application store '{AppStore}': there is no tenant namespace '{TenantId}'")]
    internal static partial void UnknownTenant(this ILogger<AppEventTranslator> logger, string eventType, EventSourceId noteId, TenantId tenantId, EventStoreName appStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for note '{NoteId}' from application store '{AppStore}': more than one namespace is named '{TenantId}', so the tenant can not be told")]
    internal static partial void AmbiguousTenant(this ILogger<AppEventTranslator> logger, string eventType, EventSourceId noteId, TenantId tenantId, EventStoreName appStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for note '{NoteId}' from application store '{AppStore}': no such note was received in tenant '{TenantId}'")]
    internal static partial void UnknownNote(this ILogger<AppEventTranslator> logger, string eventType, EventSourceId noteId, TenantId tenantId, EventStoreName appStore);

    [LoggerMessage(LogLevel.Debug, "'{EventType}' for note '{NoteId}' in tenant '{TenantId}' from application store '{AppStore}' was already recorded")]
    internal static partial void AlreadyRecorded(this ILogger<AppEventTranslator> logger, string eventType, EventSourceId noteId, TenantId tenantId, EventStoreName appStore);
}
