// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Alerts;

internal static partial class AlertDeliveryLogMessages
{
    [LoggerMessage(LogLevel.Information, "delivery {Channel}: succeeded")]
    internal static partial void Delivered(this ILogger<AlertDelivery> logger, string channel);

    [LoggerMessage(LogLevel.Warning, "delivery {Channel}: failed")]
    internal static partial void DeliveryFailed(this ILogger<AlertDelivery> logger, string channel);

    [LoggerMessage(LogLevel.Warning, "delivery: no channel is configured")]
    internal static partial void NoChannels(this ILogger<AlertDelivery> logger);
}
