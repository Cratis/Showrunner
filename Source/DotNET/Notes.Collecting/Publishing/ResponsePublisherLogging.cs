// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Collecting.Publishing;

/// <summary>
/// Log messages for <see cref="ResponsePublisher"/>.
/// </summary>
internal static partial class ResponsePublisherLogging
{
    [LoggerMessage(LogLevel.Warning, "Not publishing '{PublicEventType}' for note '{NoteId}' in namespace '{Namespace}': the same fact was already published with different content")]
    internal static partial void AlreadyPublishedDifferently(this ILogger<ResponsePublisher> logger, string publicEventType, EventSourceId noteId, EventStoreNamespaceName @namespace);
}
