// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Payments.Mollie;

internal static partial class MollieWebhookLog
{
    [LoggerMessage(LogLevel.Warning, "Could not read payment {PaymentId} from Mollie - Mollie delivers the webhook again")]
    internal static partial void CouldNotReadPayment(this ILogger logger, string paymentId, Exception exception);

    [LoggerMessage(LogLevel.Error, "The handler failed for payment {PaymentId} - Mollie delivers the webhook again")]
    internal static partial void CouldNotHandlePayment(this ILogger logger, string paymentId, Exception exception);
}
