// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.GuestList.Authoring.Publishing;

/// <summary>
/// Log messages for <see cref="GuestListOutbox"/>.
/// </summary>
internal static partial class GuestListOutboxLogging
{
    [LoggerMessage(LogLevel.Warning, "Not publishing '{PublicEventType}' for product '{Product}' in namespace '{Namespace}': the same decision was already published with different content")]
    internal static partial void AlreadyPublishedDifferently(this ILogger<GuestListOutbox> logger, string publicEventType, EventSourceId product, EventStoreNamespaceName @namespace);
}
