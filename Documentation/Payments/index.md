---
title: Payments
description: Take payments in an application through a provider-neutral contract - customers, first payments, recurring charges, subscriptions and refunds - with Mollie as the first provider and one webhook that always reads the payment back.
---

`Cratis.Showrunner.Payments` is the contract an application takes payments through. It names what an application asks of a payment provider and what it hears back, and says nothing about any one provider. `Cratis.Showrunner.Payments.Mollie` implements it against [Mollie](https://www.mollie.com/), including the webhook Mollie calls when a payment changes.

## Add Mollie to an application

```csharp
builder.Services.AddMolliePayments(builder.Configuration);
builder.Services.AddScoped<IPaymentWebhookHandler, SubscriptionPayments>();

app.MapMolliePaymentWebhook();
```

`AddMolliePayments` reads the `Mollie` configuration section:

| Option | Default | Meaning |
| --- | --- | --- |
| `ApiKey` | none | Mollie's API key. Keep it in a secret store, never in source. Without one `IPaymentProvider.IsConfigured` is `false`, every call throws and the webhook answers 404. |
| `ApiBaseUrl` | `https://api.mollie.com/v2` | Where Mollie's API is. |

## Ask for payments

Inject `IPaymentProvider`. Amounts are a `PaymentAmount` in the currency's major unit, so €20 is `new(20m, "EUR")`.

| To | Call |
| --- | --- |
| Register a customer | `CreateCustomer` |
| Send a customer to pay for the first time, leaving permission to charge them again | `CreateFirstPayment` - the returned `Payment.CheckoutUrl` is where to send them |
| Take a single payment that sets nothing up for later | `CreateOneOffPayment` |
| Charge a customer who already gave permission | `ChargeMandate` |
| Read a payment | `GetPayment` |
| Refund a payment, wholly or partly | `Refund` |
| Run a subscription | `CreateSubscription`, `GetSubscription`, `GetSubscriptions`, `UpdateSubscription`, `CancelSubscription` |

Everything you attach as `Metadata` on a payment, subscription or customer comes back on every read. Use it to find your own record again, such as the tenant or the order.

A call throws `PaymentProviderException` when the provider refuses or cannot be reached; it carries the status code and what the provider answered.

## Hear that a payment changed

`MapMolliePaymentWebhook` maps `POST /webhooks/mollie`. The callback Mollie sends carries only a payment id, so the endpoint reads the payment back from Mollie before anything else: a forged callback can name a payment but never change what it says. It then calls your `IPaymentWebhookHandler` with a `PaymentUpdate`.

```csharp
public class SubscriptionPayments : IPaymentWebhookHandler
{
    public async Task Handle(PaymentUpdate update, CancellationToken cancellationToken)
    {
        var payment = update.Payment;
        if (payment is { Status: PaymentStatus.Paid, Sequence: PaymentSequence.First })
        {
            // Activate what the customer bought. Metadata carries your own identifiers.
        }
    }
}
```

The same update can arrive more than once, so handling must be idempotent. Throw to have Mollie deliver it again. The endpoint answers 400 for a callback without an id, 502 when Mollie cannot be read yet, 500 when your handler throws and 200 otherwise.

Anything after the route is handed on as `update.Scope`: `MapMolliePaymentWebhook()` also accepts `POST /webhooks/mollie/{scope}`, which lets an application keep webhook addresses it already gave Mollie.
