// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing;

/// <summary>
/// Log messages for <see cref="MarqueeOutbox"/>.
/// </summary>
internal static partial class MarqueeOutboxLogging
{
    [LoggerMessage(LogLevel.Warning, "Not publishing '{PublicEventType}' for announcement '{AnnouncementId}' in namespace '{Namespace}': the same change was already published with different content")]
    internal static partial void AlreadyPublishedDifferently(this ILogger<MarqueeOutbox> logger, string publicEventType, EventSourceId announcementId, EventStoreNamespaceName @namespace);

    [LoggerMessage(LogLevel.Warning, "Not publishing '{PublicEventType}' for announcement '{AnnouncementId}' in namespace '{Namespace}': the announcement was never authored there")]
    internal static partial void NeverAuthored(this ILogger<MarqueeOutbox> logger, string publicEventType, EventSourceId announcementId, EventStoreNamespaceName @namespace);
}
