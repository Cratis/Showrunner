---
title: Payments reference
description: Look up the Payments contract types, enums and exceptions, the Mollie options and status mapping, and the webhook routes and the answers they give.
---

## Packages and namespaces

| Package | Namespace |
| --- | --- |
| `Cratis.Showrunner.Payments` | `Cratis.Showrunner.Payments` |
| `Cratis.Showrunner.Payments.Mollie` | `Cratis.Showrunner.Payments.Mollie` |

## IPaymentProvider

| Member | Returns | Purpose |
| --- | --- | --- |
| `IsConfigured` | `bool` | Whether the provider has what it needs to take requests |
| `CreateCustomer(NewCustomer)` | `CustomerId` | Registers a customer |
| `CreateFirstPayment(PaymentRequest)` | `Payment` | Starts a first payment that also leaves permission to charge again |
| `CreateOneOffPayment(PaymentRequest)` | `Payment` | Starts a single payment that sets nothing up for later |
| `ChargeMandate(MandateCharge)` | `Payment` | Charges against permission the customer already gave |
| `GetPayment(PaymentId)` | `Payment` | Reads a payment as the provider has it now |
| `Refund(PaymentId, PaymentAmount)` | none | Refunds a payment wholly or in part |
| `GetSubscriptions(CustomerId)` | `IReadOnlyList<RecurringSubscription>` | Lists a customer's subscriptions |
| `GetSubscription(CustomerId, SubscriptionId)` | `RecurringSubscription` | Reads one subscription |
| `CreateSubscription(SubscriptionRequest)` | `RecurringSubscription` | Starts a subscription |
| `UpdateSubscription(CustomerId, SubscriptionId, PaymentAmount, string, IReadOnlyDictionary<string, string>)` | none | Changes what a subscription charges |
| `CancelSubscription(CustomerId, SubscriptionId)` | none | Stops a subscription charging |

Every method takes an optional `CancellationToken`.

## Requests and results

| Type | Members |
| --- | --- |
| `PaymentAmount` | `Value` (decimal, major unit), `Currency` (ISO code such as `EUR`) |
| `NewCustomer` | `Name`, `Email?`, `Metadata` |
| `PaymentRequest` | `Amount`, `Description`, `RedirectUrl`, `WebhookUrl?`, `Metadata`, `CustomerId?` |
| `MandateCharge` | `CustomerId`, `Amount`, `Description`, `WebhookUrl?`, `Metadata` |
| `SubscriptionRequest` | `CustomerId`, `Amount`, `Interval`, `Description`, `WebhookUrl?`, `Metadata` |
| `Payment` | `Id`, `Status`, `Amount`, `Sequence`, `CustomerId?`, `SubscriptionId?`, `MandateId?`, `CheckoutUrl?`, `Metadata` |
| `RecurringSubscription` | `Id`, `Status` |
| `PaymentUpdate` | `Payment`, `Scope?` |

`CustomerId`, `PaymentId` and `SubscriptionId` are concepts over the provider's identifiers, and convert from and to `string`.

## Enums

| Enum | Values |
| --- | --- |
| `PaymentStatus` | `Open`, `Paid`, `Failed`, `Canceled`, `Expired` |
| `PaymentSequence` | `OneOff`, `First`, `Recurring` |
| `PaymentInterval` | `Monthly`, `Yearly` |
| `SubscriptionStatus` | `Active`, `Canceled`, `Suspended`, `Other` |

## IPaymentWebhookHandler

`Task Handle(PaymentUpdate update, CancellationToken cancellationToken)`. Must be idempotent. Throw to have the provider deliver the update again.

## PaymentProviderException

| Member | Meaning |
| --- | --- |
| `Operation` | The operation that failed |
| `StatusCode` | The provider's HTTP status code, or zero when there was no answer |
| `Body` | What the provider answered |

## Mollie

| Item | Value |
| --- | --- |
| Configuration section | `Mollie` |
| `ApiKey` | Mollie's API key; empty means not configured |
| `ApiBaseUrl` | `https://api.mollie.com/v2` |
| Registration | `AddMolliePayments(IConfiguration)` or `AddMolliePayments(Action<MollieOptions>)` |
| Webhook | `MapMolliePaymentWebhook(route = "/webhooks/mollie")`, mapping `POST {route}` and `POST {route}/{scope}` |
| HTTP client timeout | 30 seconds |

### How Mollie maps to the contract

| Contract | Mollie |
| --- | --- |
| `PaymentStatus.Paid`, `Failed`, `Canceled`, `Expired` | `paid`, `failed`, `canceled`, `expired`; every other status is `Open` |
| `PaymentSequence.First`, `Recurring`, `OneOff` | `first`, `recurring`; anything else is `OneOff` |
| `SubscriptionStatus.Active`, `Canceled`, `Suspended` | `active`, `canceled`, `suspended`; every other status is `Other` |
| `PaymentInterval.Monthly`, `Yearly` | `1 month`, `12 months` |
