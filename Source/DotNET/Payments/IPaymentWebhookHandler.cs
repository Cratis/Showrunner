// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// What an application does when the provider reports that a payment changed. Register one implementation; the
/// provider's webhook endpoint calls it once the payment has been read back.
/// </summary>
public interface IPaymentWebhookHandler
{
    /// <summary>
    /// Handles a payment that changed. The same update can arrive more than once, so handling must be idempotent;
    /// throw to have the provider deliver it again.
    /// </summary>
    /// <param name="update">The update.</param>
    /// <param name="cancellationToken">Cancels the handling.</param>
    /// <returns>A task that completes when the update was handled.</returns>
    Task Handle(PaymentUpdate update, CancellationToken cancellationToken);
}
