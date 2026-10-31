// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing;

/// <summary>
/// Log messages for <see cref="OfferPublisher"/>.
/// </summary>
internal static partial class OfferPublisherLogging
{
    [LoggerMessage(LogLevel.Warning, "Not publishing '{PublicEventType}' for offer '{OfferId}' in namespace '{Namespace}': the same change was already published with different content")]
    internal static partial void AlreadyPublishedDifferently(this ILogger<OfferPublisher> logger, string publicEventType, EventSourceId offerId, EventStoreNamespaceName @namespace);
}
