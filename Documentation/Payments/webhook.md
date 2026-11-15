---
title: Hear that a payment changed
description: Implement IPaymentWebhookHandler, understand how the Mollie webhook verifies a callback, and handle repeated, failed and scoped deliveries.
---

A payment finishes after your code has returned: the customer pays later, a bank settles later, a recurring charge runs on the provider's schedule. The provider tells you by calling a webhook, and the webhook tells your code through `IPaymentWebhookHandler`.

## How the endpoint works

`MapMolliePaymentWebhook` maps `POST /webhooks/mollie`. The callback Mollie sends carries only a payment id, not the payment. The endpoint therefore reads the payment back from Mollie before anything else: a forged callback can name a payment but never change what it says. It then calls your handler with a `PaymentUpdate`.

## Write the handler

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

Register it with `builder.Services.AddScoped<IPaymentWebhookHandler, SubscriptionPayments>()`.

Branch on `Payment.Status` and `Payment.Sequence`:

| Situation | Check |
| --- | --- |
| The customer paid for the first time | `Status` is `Paid` and `Sequence` is `First`. The `MandateId` is now set, so you can charge them again. |
| A one-off purchase was paid | `Status` is `Paid` and `Sequence` is `OneOff` |
| A subscription or mandate charge was paid | `Status` is `Paid` and `Sequence` is `Recurring`; `SubscriptionId` is set for a subscription |
| A payment did not go through | `Status` is `Failed`, `Canceled` or `Expired` |

## Handle repeated deliveries

The same update can arrive more than once, so handling must be idempotent. Key your work on `Payment.Id` and the status, and do nothing when you already applied it.

Throw to have Mollie deliver the update again. Mollie delivers again on anything but a 2xx answer.

## What the endpoint answers

| Answer | When |
| --- | --- |
| 200 | Your handler completed. |
| 400 | The callback carried no id. |
| 404 | Mollie is not configured: there is no API key. |
| 500 | Your handler threw. |
| 502 | Mollie could not be read yet. |

## Keep webhook addresses you already gave Mollie

Anything after the route is handed on as `update.Scope`. `MapMolliePaymentWebhook()` also accepts `POST /webhooks/mollie/{scope}`, which lets an application keep webhook addresses it already gave Mollie and tell them apart in the handler. `Scope` is `null` when nothing follows the route.
