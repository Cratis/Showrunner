// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Publishing;

/// <summary>
/// Log messages for <see cref="OutboxPublisher"/>.
/// </summary>
internal static partial class OutboxPublisherLogging
{
    [LoggerMessage(LogLevel.Warning, "Not publishing '{PublicEventType}' for issue '{IssueId}' in namespace '{Namespace}': the same fact was already published with different content")]
    internal static partial void AlreadyPublishedDifferently(this ILogger<OutboxPublisher> logger, string publicEventType, EventSourceId issueId, EventStoreNamespaceName @namespace);
}
