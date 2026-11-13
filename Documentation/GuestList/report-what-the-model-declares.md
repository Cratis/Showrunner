---
title: Report what the model declares
description: Have a product tell its operator which audiences, blocks and gates its availability model declares and which decisions it recorded, and read both on the operator side.
---

A product that evaluates availability with [Cratis Choreography](https://github.com/Cratis/Choreography) already knows what its model declares. This page makes it tell the operator, so the operator can show what can be granted, since when, and whether a decision has reached the product. Nothing is polled, nothing ticks and the operator never claims the product is connected: the product reports a change when there is one.

## Report from the product

The reporting is part of `Cratis.Showrunner.GuestList.Availability`. Register it as described in [Feed the availability engine](./feed-the-availability-engine.md); it adds `AvailabilityReporter` and a hosted service that runs it.

```csharp
builder.Services.AddGuestList(options => options.Product = "ledger");
builder.Services.AddGuestListAvailability(options => options.EnvironmentName = "Production");
```

The reporter reads `IAvailabilityManifest`, the description of the model that Choreography 2.8 added, and appends `AvailabilityDeclared` to the outbox, see [Contracts](./contracts.md#product-events). Register the manifest the model's generated code provides; without one the manifest is empty, the product declares nothing and nothing is reported.

- **Once per change.** What was last reported is read from the outbox, so a restart, a second instance or a build that declares the same reports nothing again. Declaring the same things in another order is not a change, and neither is a new `Version`.
- **Never in the way of the host.** Reporting runs in the background. When Chronicle cannot be reached it tries again every `RetryInterval` and the host starts regardless.
- **Idempotent.** Each report carries a key made from its content and the report it follows, so two instances starting together append one.

| Option of `GuestListOptions` | Default | What it does |
| --- | --- | --- |
| `Environment` | the engine's `EnvironmentName` | The environment the product reports from, told with everything it reports |
| `Version` | the entry assembly's informational version | The version told with the declaration |
| `OperatorAudience` | none | The routing audience of the operator, when it listens for its own |
| `RetryInterval` | 30 seconds | How long to wait before reporting again after a failure |

`EveryDeclaredAudienceGranted` is taken from the engine's `GrantAllDeclaredAudiences` switch, so the operator can say that an environment grants everything whatever it decided.

### Tell the operator which decisions arrived

For every decision the product records, it appends one `AudienceDecisionReceived` with `DecidedAt` set to when the operator decided. It follows what the product recorded, so a decision that is delivered again, or one the product ignored such as one for a tenant it does not have, produces no receipt and stays waiting. This part is in `Cratis.Showrunner.GuestList` itself and needs no Choreography.

## Read it as an operator

The operator's host subscribes to the product's published events, as it does for any product events, and appends them to the event log that the authoring read models read. The wiring belongs to the host. It then has:

| Read model | Built from | Answers |
| --- | --- | --- |
| `ProductAvailability` | `AvailabilityDeclared` | What the product declares in each environment, and what each report added, removed or changed |
| `AudienceDelivery` | `AudienceDecisionReceived` | For each environment, the newest decision it received |

`ProductAvailability` keeps the last declaration of each environment, ordered by environment name, and a list of changes, oldest first. A change lists the audiences, blocks, gates, context values and personas that were added, removed or declared differently. The first report adds everything.

`AudienceDelivery` keeps a high-water mark and not a ledger. Chronicle delivers the decisions of a product in order, so a decision is waiting in an environment when it was taken after `ReceivedThrough`:

```csharp
var waiting = delivery.Environments.Single(_ => _.Environment == "Production").IsWaiting(decision.DecidedAt);
```

An environment that has not received anything is waiting for every decision. That is all the operator knows; it is not a statement that the product is down.

The undeclared grants report uses the product's own declaration for the primary environment, set with `PrimaryEnvironment`, and falls back to `DeclaredAudiences` when the product has not reported.

## When it does not report

| You see | Cause |
| --- | --- |
| `ProductAvailability` is empty for a product | The product has not reported: the model declares nothing, no manifest is registered, or the product is not running a version that reports |
| A decision stays waiting | The product has not recorded it. A tenant the product does not have is ignored and logged by the product |
| A change is not shown | Only a different declaration is a change; a new version, or the same things in another order, is not |
