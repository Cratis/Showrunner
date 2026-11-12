// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.GuestList.Receiving;

/// <summary>
/// Log messages for <see cref="GuestListInbox"/>.
/// </summary>
internal static partial class GuestListInboxLogging
{
    [LoggerMessage(LogLevel.Debug, "Ignoring '{EventType}' from operator store '{OperatorStore}': it is for the product '{Product}', not this one")]
    internal static partial void ForAnotherProduct(this ILogger<GuestListInbox> logger, string eventType, EventStoreName operatorStore, string product);

    [LoggerMessage(LogLevel.Warning, "Not recording '{EventType}' from operator store '{OperatorStore}': it names no audience, or no tenant or user for the level '{Level}'")]
    internal static partial void Malformed(this ILogger<GuestListInbox> logger, string eventType, EventStoreName operatorStore, AudienceLevel level);

    [LoggerMessage(LogLevel.Warning, "Not recording '{EventType}' from operator store '{OperatorStore}': the product has no tenant '{Tenant}', and a decision never creates a tenant")]
    internal static partial void UnknownTenant(this ILogger<GuestListInbox> logger, string eventType, EventStoreName operatorStore, string tenant);

    [LoggerMessage(LogLevel.Debug, "Not recording '{EventType}' for '{EventSourceId}' in namespace '{Namespace}' from operator store '{OperatorStore}': it was already recorded")]
    internal static partial void AlreadyRecorded(this ILogger<GuestListInbox> logger, string eventType, EventSourceId eventSourceId, EventStoreNamespaceName @namespace, EventStoreName operatorStore);
}
