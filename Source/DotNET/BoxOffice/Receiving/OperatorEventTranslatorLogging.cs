// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Receiving;

internal static partial class OperatorEventTranslatorLogging
{
    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' from operator store '{OperatorStore}': it is not a decision Box Office receives")]
    internal static partial void NotADecision(this ILogger<OperatorEventTranslator> logger, string eventType, EventStoreName operatorStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for organization '{Organization}' from operator store '{OperatorStore}': it names no tenant")]
    internal static partial void NoTenant(this ILogger<OperatorEventTranslator> logger, string eventType, EventSourceId organization, EventStoreName operatorStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for organization '{Organization}' from operator store '{OperatorStore}': there is no tenant namespace '{TenantId}'")]
    internal static partial void UnknownTenant(this ILogger<OperatorEventTranslator> logger, string eventType, EventSourceId organization, TenantId tenantId, EventStoreName operatorStore);

    [LoggerMessage(LogLevel.Warning, "Ignoring '{EventType}' for organization '{Organization}' from operator store '{OperatorStore}': more than one namespace is named '{TenantId}', so the tenant can not be told")]
    internal static partial void AmbiguousTenant(this ILogger<OperatorEventTranslator> logger, string eventType, EventSourceId organization, TenantId tenantId, EventStoreName operatorStore);

    [LoggerMessage(LogLevel.Debug, "'{EventType}' for '{EventSource}' in namespace '{Namespace}' from operator store '{OperatorStore}' was already recorded")]
    internal static partial void AlreadyRecorded(this ILogger<OperatorEventTranslator> logger, string eventType, EventSourceId eventSource, EventStoreNamespaceName @namespace, EventStoreName operatorStore);
}
