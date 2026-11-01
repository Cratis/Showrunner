// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Contracts.Transport;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Transport;

/// <summary>
/// Log messages for the HTTP transport. None of them carries the secret, a request or a note.
/// </summary>
internal static partial class TransportLogging
{
    [LoggerMessage(LogLevel.Warning, "Ignoring the collector's answer {Cursor} for note '{NoteId}': no tenant namespace holds that note")]
    internal static partial void UnknownNote(this ILogger<ResponseApplier> logger, Guid noteId, ulong cursor);

    [LoggerMessage(LogLevel.Warning, "Ignoring the collector's {Kind} answer {Cursor} for note '{NoteId}': it is incomplete")]
    internal static partial void UnusableResponse(this ILogger<ResponseApplier> logger, Guid noteId, ulong cursor, NoteResponseKind kind);

    [LoggerMessage(LogLevel.Debug, "The collector's answer {Cursor} for note '{NoteId}' was already applied")]
    internal static partial void AlreadyApplied(this ILogger<ResponseApplier> logger, Guid noteId, ulong cursor);

    [LoggerMessage(LogLevel.Warning, "Relaying notes to the collector failed; trying again in {RetryInSeconds} seconds")]
    internal static partial void RelayFailed(this ILogger<CollectorRelayWorker> logger, Exception exception, int retryInSeconds);
}
