// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Collecting.Transport;

/// <summary>
/// Log messages for <see cref="NoteCollector"/>. None carries the secret or the content of a note.
/// </summary>
internal static partial class NoteCollectorLogging
{
    [LoggerMessage(LogLevel.Warning, "Rejected note {SequenceNumber} of publication '{PublicationId}': it names another tenant than the publication does")]
    internal static partial void TenantMismatch(this ILogger<NoteCollector> logger, PublicationId publicationId, ulong sequenceNumber);

    [LoggerMessage(LogLevel.Warning, "Rejected a batch of publication '{PublicationId}': its tenant has no single namespace in the collecting store")]
    internal static partial void UnknownTenant(this ILogger<NoteCollector> logger, PublicationId publicationId);

    [LoggerMessage(LogLevel.Warning, "Rejected note {SequenceNumber} of publication '{PublicationId}': it could not be read")]
    internal static partial void Unreadable(this ILogger<NoteCollector> logger, PublicationId publicationId, ulong sequenceNumber);

    [LoggerMessage(LogLevel.Warning, "Ignoring note {SequenceNumber} of publication '{PublicationId}': '{EventType}' is not an event an application sends")]
    internal static partial void NotAnApplicationEvent(this ILogger<NoteCollector> logger, PublicationId publicationId, ulong sequenceNumber, string eventType);

    [LoggerMessage(LogLevel.Debug, "Stored {Count} note(s) of publication '{PublicationId}' up to {SequenceNumber}")]
    internal static partial void Stored(this ILogger<NoteCollector> logger, PublicationId publicationId, int count, ulong sequenceNumber);
}
