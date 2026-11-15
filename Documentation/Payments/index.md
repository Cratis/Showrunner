---
title: Payments
description: Take payments through a provider-neutral contract for customers, first and recurring payments, subscriptions and refunds, with Mollie first and one webhook.
---

`Cratis.Showrunner.Payments` is the contract an application takes payments through. It names what an application asks of a payment provider and what it hears back, and says nothing about any one provider. `Cratis.Showrunner.Payments.Mollie` implements it against [Mollie](https://www.mollie.com/), including the webhook Mollie calls when a payment changes.

Without it, each product that charges customers writes its own provider client, its own mapping of provider statuses and its own webhook, and ties its billing code to one provider. With it, your code asks `IPaymentProvider` for a payment and handles a `PaymentUpdate`, and the provider behind it is a registration choice.

Payments is a library, not a hosted service. It keeps no state of its own: the provider holds customers, payments and subscriptions, and you keep your own records, found again through the `Metadata` you attach.

## Packages

| Package | What it is |
| --- | --- |
| `Cratis.Showrunner.Payments` | The contract: `IPaymentProvider`, its requests and results, and `IPaymentWebhookHandler` |
| `Cratis.Showrunner.Payments.Mollie` | The Mollie implementation of the contract and its webhook endpoint |

## How a payment moves

1. Your application registers a customer with `CreateCustomer`.
2. It sends the customer to pay for the first time with `CreateFirstPayment`. The returned `Payment.CheckoutUrl` is where the customer pays, and paying leaves permission to charge them again.
3. The provider calls your webhook when the payment changes. The endpoint reads the payment back from the provider and hands your `IPaymentWebhookHandler` a `PaymentUpdate`.
4. From then on you charge the customer with `ChargeMandate`, or start a `CreateSubscription` that the provider runs on schedule. Each charge reports back through the same webhook.

## Where to go next

| To | Read |
| --- | --- |
| Add Mollie to an application and configure it | [Add Mollie to an application](./add-mollie-to-an-application.md) |
| Create customers, take payments and refund them | [Take and manage payments](./take-and-manage-payments.md) |
| Run recurring billing | [Subscriptions](./subscriptions.md) |
| Hear that a payment changed | [Hear that a payment changed](./webhook.md) |
| Look up a type, status or option | [Reference](./reference.md) |
