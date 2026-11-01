// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Receiving;

/// <summary>
/// Log messages for <see cref="ReceiverEventTranslator"/>.
/// </summary>
internal static partial class ReceiverEventTranslatorLogging
{
    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' from collecting store '{CollectingStore}': it is not an event Notes receives")]
    internal static partial void NotAnEventFromTheReceiver(this ILogger<ReceiverEventTranslator> logger, string eventType, EventStoreName collectingStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for note '{NoteId}' from collecting store '{CollectingStore}': it names no tenant")]
    internal static partial void NoTenant(this ILogger<ReceiverEventTranslator> logger, string eventType, EventSourceId noteId, EventStoreName collectingStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for note '{NoteId}' from collecting store '{CollectingStore}': there is no tenant namespace '{TenantId}'")]
    internal static partial void UnknownTenant(this ILogger<ReceiverEventTranslator> logger, string eventType, EventSourceId noteId, TenantId tenantId, EventStoreName collectingStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for note '{NoteId}' from collecting store '{CollectingStore}': more than one namespace is named '{TenantId}', so the tenant can not be told")]
    internal static partial void AmbiguousTenant(this ILogger<ReceiverEventTranslator> logger, string eventType, EventSourceId noteId, TenantId tenantId, EventStoreName collectingStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for note '{NoteId}' from collecting store '{CollectingStore}': no such note was given in tenant '{TenantId}'")]
    internal static partial void UnknownNote(this ILogger<ReceiverEventTranslator> logger, string eventType, EventSourceId noteId, TenantId tenantId, EventStoreName collectingStore);

    [LoggerMessage(LogLevel.Debug, "'{EventType}' for note '{NoteId}' in tenant '{TenantId}' from collecting store '{CollectingStore}' was already recorded")]
    internal static partial void AlreadyRecorded(this ILogger<ReceiverEventTranslator> logger, string eventType, EventSourceId noteId, TenantId tenantId, EventStoreName collectingStore);
}
