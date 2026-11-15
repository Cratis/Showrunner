---
title: Subscriptions
description: Start, read, change and cancel recurring subscriptions that the payment provider charges on a monthly or yearly schedule.
---

A subscription is a charge the provider repeats on a schedule against a customer's permission. You do not call `ChargeMandate` each period: the provider charges, and every payment it takes arrives at your [webhook](./webhook.md) as a `PaymentUpdate` with `Sequence` set to `Recurring` and `SubscriptionId` set.

A subscription needs a customer who gave permission through a paid first payment. See [Take and manage payments](./take-and-manage-payments.md).

| To | Call |
| --- | --- |
| Start a subscription | `CreateSubscription` |
| Read one subscription | `GetSubscription` |
| List a customer's subscriptions | `GetSubscriptions` |
| Change what each period costs | `UpdateSubscription` |
| Stop it charging | `CancelSubscription` |

## Start a subscription

```csharp
var subscription = await payments.CreateSubscription(
    new SubscriptionRequest(
        customer,
        new PaymentAmount(20m, "EUR"),
        PaymentInterval.Monthly,
        "Starter plan",
        WebhookUrl: "https://app.example/webhooks/mollie",
        Metadata: new Dictionary<string, string> { ["tenant"] = tenantId }),
    cancellationToken);
```

`PaymentInterval` is `Monthly` (every month) or `Yearly` (every twelve months). The returned `RecurringSubscription` carries the subscription's `Id` and `Status`. Keep the id against your own account.

## Change a subscription

```csharp
await payments.UpdateSubscription(customer, subscription.Id, new PaymentAmount(30m, "EUR"), "Pro plan", metadata, cancellationToken);
```

The new amount applies from the next charge. The description is what the customer sees on every payment.

## Cancel a subscription

```csharp
await payments.CancelSubscription(customer, subscription.Id, cancellationToken);
```

## Read the status

`SubscriptionStatus` is one of:

| Status | Meaning |
| --- | --- |
| `Active` | The subscription charges on schedule. |
| `Canceled` | The subscription was canceled and charges no more. |
| `Suspended` | The subscription is paused, for example because the customer's permission to charge ran out. |
| `Other` | A state the contract does not name: not started yet or completed. |

A `Suspended` subscription charges nothing until the cause is resolved. Read the status after a failed payment so your application can tell the customer.
