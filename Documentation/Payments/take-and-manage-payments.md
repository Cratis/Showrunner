---
title: Take and manage payments
description: Register customers, take first and one-off payments, charge a customer who already gave permission, read payments and refund them through IPaymentProvider.
---

Inject `IPaymentProvider` wherever you take payments. Amounts are a `PaymentAmount` in the currency's major unit, so €20 is `new(20m, "EUR")`.

| To | Call |
| --- | --- |
| Register a customer | `CreateCustomer` |
| Send a customer to pay for the first time, leaving permission to charge them again | `CreateFirstPayment` |
| Take a single payment that sets nothing up for later | `CreateOneOffPayment` |
| Charge a customer who already gave permission | `ChargeMandate` |
| Read a payment | `GetPayment` |
| Refund a payment, wholly or partly | `Refund` |

## Register a customer

```csharp
CustomerId customer = await payments.CreateCustomer(
    new NewCustomer("Acme Ltd", "billing@acme.example", new Dictionary<string, string> { ["tenant"] = tenantId }),
    cancellationToken);
```

The provider returns the customer's identity. Store it against your own tenant or account.

## Take the first payment

A first payment is the one that gives you permission to charge the customer again. Send the customer to its checkout:

```csharp
var payment = await payments.CreateFirstPayment(
    new PaymentRequest(
        new PaymentAmount(20m, "EUR"),
        "Starter plan",
        RedirectUrl: "https://app.example/billing/done",
        WebhookUrl: "https://app.example/webhooks/mollie",
        Metadata: new Dictionary<string, string> { ["tenant"] = tenantId },
        CustomerId: customer),
    cancellationToken);

return Results.Redirect(payment.CheckoutUrl!);
```

`RedirectUrl` is where the customer lands after paying. `WebhookUrl` is where the provider reports the result; it is optional when the provider already knows where to call. The payment is `Open` until the customer pays, so do not treat the redirect as proof of payment. Wait for the [webhook](./webhook.md).

## Take a one-off payment

`CreateOneOffPayment` takes the same `PaymentRequest` but sets nothing up for later, so you cannot charge the customer again from it. Use it for a single purchase.

## Charge a customer who already gave permission

After a paid first payment, charge the customer without sending them to a checkout:

```csharp
var payment = await payments.ChargeMandate(
    new MandateCharge(
        customer,
        new PaymentAmount(20m, "EUR"),
        "Starter plan, June",
        WebhookUrl: "https://app.example/webhooks/mollie",
        Metadata: new Dictionary<string, string> { ["tenant"] = tenantId }),
    cancellationToken);
```

The payment can still fail later, for example if the customer's bank refuses the charge, so rely on the webhook for the result. For charges that repeat on a schedule, a [subscription](./subscriptions.md) is simpler.

## Read a payment

`GetPayment` returns the payment as the provider has it now. Use it to reconcile, or after a webhook you want to double-check.

## Refund a payment

```csharp
await payments.Refund(paymentId, new PaymentAmount(5m, "EUR"), cancellationToken);
```

Pass the full amount for a whole refund or a smaller one for a partial refund. The call returns when the provider took the refund.

## Carry your own identifiers in metadata

Everything you attach as `Metadata` on a payment, subscription or customer comes back on every read. Use it to find your own record again, such as the tenant or the order.

## Handle failures

A call throws `PaymentProviderException` when the provider refuses or cannot be reached. It carries the `Operation`, the `StatusCode` (zero when there was no answer) and the `Body` the provider answered. When Mollie has no API key, every call throws.
