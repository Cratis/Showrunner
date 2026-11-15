---
title: Add Mollie to an application
description: Register Mollie as the payment provider, configure the API key, map the webhook endpoint and verify that the application can take payments.
---

This page adds Mollie to an ASP.NET Core application. Before you start, have a Mollie account and an API key. Use a test key while you build.

## Add the package

```shell
dotnet add package Cratis.Showrunner.Payments.Mollie
```

The package brings `Cratis.Showrunner.Payments` with it.

## Register the provider and the webhook

```csharp
builder.Services.AddMolliePayments(builder.Configuration);
builder.Services.AddScoped<IPaymentWebhookHandler, SubscriptionPayments>();

app.MapMolliePaymentWebhook();
```

- `AddMolliePayments` registers Mollie as the application's `IPaymentProvider`, backed by an `HttpClient` with a 30 second timeout. It has an overload that takes an `Action<MollieOptions>` instead of the configuration.
- `IPaymentWebhookHandler` is your code. Without one the webhook has nobody to tell. See [Hear that a payment changed](./webhook.md).
- `MapMolliePaymentWebhook` maps `POST /webhooks/mollie`, and `POST /webhooks/mollie/{scope}`. Pass a route to use another address. The endpoints allow anonymous callers, because Mollie calls them without credentials; the endpoint protects itself by reading every payment back from Mollie.

## Configure it

`AddMolliePayments(configuration)` reads the `Mollie` configuration section:

| Option | Default | Meaning |
| --- | --- | --- |
| `ApiKey` | none | Mollie's API key. Keep it in a secret store, never in source. Without one `IPaymentProvider.IsConfigured` is `false`, every call throws and the webhook answers 404. |
| `ApiBaseUrl` | `https://api.mollie.com/v2` | Where Mollie's API is. Change it only to point at a stand-in during testing. |

```json
{
  "Mollie": {
    "ApiKey": "test_..."
  }
}
```

Set the key from the environment in a deployed application, for example `Mollie__ApiKey`.

## Make the webhook reachable

Mollie calls the webhook over the public internet, so the address you pass as `WebhookUrl` on a payment or subscription must be reachable from Mollie. During local development, expose the application with a tunnel and use that address. The webhook route must also be allowed through any gateway in front of the application, because Mollie sends no credentials.

## Check it works

Create a one-off payment with a small test amount, open its `CheckoutUrl`, pay with Mollie's test method, and confirm your handler receives a `PaymentUpdate` whose `Payment.Status` is `Paid`. See [Take and manage payments](./take-and-manage-payments.md).
