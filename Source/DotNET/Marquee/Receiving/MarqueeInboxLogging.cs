// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Marquee.Receiving;

/// <summary>
/// Log messages for <see cref="MarqueeInbox"/>.
/// </summary>
internal static partial class MarqueeInboxLogging
{
    [LoggerMessage(LogLevel.Debug, "Ignoring '{EventType}' for announcement '{AnnouncementId}' from authoring store '{AuthoringStore}': it is for the product '{Product}', not this one")]
    internal static partial void ForAnotherProduct(this ILogger<MarqueeInbox> logger, string eventType, EventSourceId announcementId, EventStoreName authoringStore, string product);

    [LoggerMessage(LogLevel.Information, "Ignoring '{EventType}' for announcement '{AnnouncementId}' from authoring store '{AuthoringStore}': the announcement was already retracted")]
    internal static partial void AlreadyRetracted(this ILogger<MarqueeInbox> logger, string eventType, EventSourceId announcementId, EventStoreName authoringStore);

    [LoggerMessage(LogLevel.Information, "Ignoring '{EventType}' for announcement '{AnnouncementId}' from authoring store '{AuthoringStore}': the announcement was already received")]
    internal static partial void AlreadyReceived(this ILogger<MarqueeInbox> logger, string eventType, EventSourceId announcementId, EventStoreName authoringStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for announcement '{AnnouncementId}' from authoring store '{AuthoringStore}': the announcement was never received")]
    internal static partial void NeverReceived(this ILogger<MarqueeInbox> logger, string eventType, EventSourceId announcementId, EventStoreName authoringStore);

    [LoggerMessage(LogLevel.Information, "Recording '{EventType}' for announcement '{AnnouncementId}' from authoring store '{AuthoringStore}'")]
    internal static partial void Recording(this ILogger<MarqueeInbox> logger, string eventType, EventSourceId announcementId, EventStoreName authoringStore);

    [LoggerMessage(LogLevel.Debug, "'{EventType}' for announcement '{AnnouncementId}' from authoring store '{AuthoringStore}' was already recorded")]
    internal static partial void AlreadyRecorded(this ILogger<MarqueeInbox> logger, string eventType, EventSourceId announcementId, EventStoreName authoringStore);
}
