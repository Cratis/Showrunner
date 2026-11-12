// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// The payment provider as an application reaches it. Each method throws <see cref="PaymentProviderException"/>
/// when the provider refuses or cannot be reached; check <see cref="IsConfigured"/> first when the provider is optional.
/// </summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Gets whether the provider has what it needs to take requests.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Registers a customer.
    /// </summary>
    /// <param name="customer">The customer.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The identity the provider gave the customer.</returns>
    Task<CustomerId> CreateCustomer(NewCustomer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a customer's first payment, which also leaves permission to charge them again.
    /// </summary>
    /// <param name="request">The payment.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The payment, carrying the checkout to send the customer to.</returns>
    Task<Payment> CreateFirstPayment(PaymentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a single payment that sets nothing up for later.
    /// </summary>
    /// <param name="request">The payment.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The payment, carrying the checkout to send the customer to.</returns>
    Task<Payment> CreateOneOffPayment(PaymentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Charges a customer against the permission they gave with their first payment.
    /// </summary>
    /// <param name="charge">The charge.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The payment.</returns>
    Task<Payment> ChargeMandate(MandateCharge charge, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a payment from the provider.
    /// </summary>
    /// <param name="payment">The payment.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The payment as the provider has it now.</returns>
    Task<Payment> GetPayment(PaymentId payment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refunds a payment, wholly or in part.
    /// </summary>
    /// <param name="payment">The payment.</param>
    /// <param name="amount">How much to refund.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>A task that completes when the provider took the refund.</returns>
    Task Refund(PaymentId payment, PaymentAmount amount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists a customer's subscriptions.
    /// </summary>
    /// <param name="customer">The customer.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The subscriptions.</returns>
    Task<IReadOnlyList<RecurringSubscription>> GetSubscriptions(CustomerId customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads one subscription.
    /// </summary>
    /// <param name="customer">The customer.</param>
    /// <param name="subscription">The subscription.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The subscription as the provider has it now.</returns>
    Task<RecurringSubscription> GetSubscription(CustomerId customer, SubscriptionId subscription, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a subscription.
    /// </summary>
    /// <param name="request">The subscription to start.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The subscription.</returns>
    Task<RecurringSubscription> CreateSubscription(SubscriptionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes what a subscription charges.
    /// </summary>
    /// <param name="customer">The customer.</param>
    /// <param name="subscription">The subscription.</param>
    /// <param name="amount">What each period costs from now on.</param>
    /// <param name="description">What the customer sees on every payment.</param>
    /// <param name="metadata">What to keep on the subscription and its payments.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>A task that completes when the provider took the change.</returns>
    Task UpdateSubscription(CustomerId customer, SubscriptionId subscription, PaymentAmount amount, string description, IReadOnlyDictionary<string, string> metadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops a subscription charging.
    /// </summary>
    /// <param name="customer">The customer.</param>
    /// <param name="subscription">The subscription.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>A task that completes when the provider canceled it.</returns>
    Task CancelSubscription(CustomerId customer, SubscriptionId subscription, CancellationToken cancellationToken = default);
}
