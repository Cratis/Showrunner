---
title: GuestList contracts
description: The public events an operator publishes to grant and withdraw an audience, the events a product publishes back to say what it declares and which decisions it recorded, the types they carry and how their schema is guarded.
---

`Cratis.Showrunner.GuestList.Contracts` holds the events an operator and a product exchange. It has no behavior and depends only on Chronicle. The events are `[Public]` and name no origin store, because they are shared by operator stores whose names vary by deployment; the product subscribes to the operator stores explicitly, see [Receive audiences in a product](./receive-audiences-in-a-product.md).

The operator publishes `AudienceGranted` and `AudienceWithdrawn`. The product publishes `AvailabilityDeclared` and `AudienceDecisionReceived` back, see [Product events](#product-events).

## Events

All types are in `Cratis.Showrunner.GuestList.Contracts.Audiences`.

| Event | Fields | Meaning |
| --- | --- | --- |
| `AudienceGranted` (generation 2) | `Product`; `Level: AudienceLevel`; `TenantId`; `UserId`; `Audience: AudienceName`; `ClientId: SystemClientId`; `Kind: SystemKind` | An operator grants an audience at a level |
| `AudienceWithdrawn` (generation 2) | `Product`; `Level: AudienceLevel`; `TenantId`; `UserId`; `Audience: AudienceName`; `ClientId: SystemClientId`; `Kind: SystemKind` | An operator withdraws an audience from a level |

Which of `TenantId`, `UserId`, `ClientId` and `Kind` are filled depends on the level. The others are empty (`Kind` is `None`), so the payload has no nulls.

| `Level` | Value | `TenantId` | `UserId` | `ClientId` | `Kind` |
| --- | --- | --- | --- | --- | --- |
| `Tenant` | 0 | the tenant | empty | empty | `None` |
| `User` | 1 | the user's tenant | the user | empty | `None` |
| `System` | 2 | empty | empty | empty | `None` |
| `NamedSystem` | 3 | empty | empty | the system's client id | `Mcp`, `Api` or `Other` |

`System` is every system that calls the product, and means what it always meant. `NamedSystem` is one system, identified by its client id; `Kind` describes it and is not part of who it is.

The meaning is the same for both events:

- A grant or a withdrawal affects its own level only. Withdrawing `Plus` from a user leaves the tenant's and the system's grants of `Plus` in place. Withdrawing it from one named system leaves every system's grant and every other named system's grant in place, and withdrawing it from `System` leaves the named systems' grants in place.
- A request made by a named system has the audiences granted to `System` plus its own, never another system's and never a person's.
- The newest decision about the same audience at the same level wins, judged by when the operator decided, not by when the event arrived. An older grant that arrives after a newer withdrawal does not bring the audience back.
- A decision that arrives twice is recorded once.
- An audience name the product does not declare is kept like any other. It never matches a gate and shows up in the undeclared grants report.

The events carry no event source of their own. The operator appends them under the product's name, so all decisions of a product share one stream and arrive in the order they were taken.

## Product events

The product publishes these from its own outbox, under its own name, so all of them share the stream of the product's decisions. All types are in `Cratis.Showrunner.GuestList.Contracts.Availability`. Both events are generation 1 and additive: nothing that shipped before changes.

| Event | Fields | Meaning |
| --- | --- | --- |
| `AvailabilityDeclared` | `Product`; `Environment: EnvironmentName`; `Version: ModelVersion`; `Origin`; `EveryDeclaredAudienceGranted`; `Audiences: AudienceDeclaration[]`; `Availabilities: AvailabilityBlockDeclaration[]`; `Gates: GateDeclaration[]`; `ContextValues: ContextValueDeclaration[]`; `Personas: PersonaDeclaration[]` | What the product's model declares, reported once per change |
| `AudienceDecisionReceived` | `Product`; `Environment: EnvironmentName`; `Decision: AudienceDecision`; `Level: AudienceLevel`; `TenantId`; `UserId`; `Audience: AudienceName`; `DecidedAt` | The product recorded an operator decision |

`AvailabilityDeclared` is data about the model, not about who holds what. Its parts:

| Type | Fields |
| --- | --- |
| `AudienceDeclaration` | `Name: AudienceName`; `Description`; `Source` |
| `AvailabilityBlockDeclaration` | `Name`; `Description`; `Source`; `Alternatives: AvailabilityAlternative[]` |
| `GateDeclaration` | `Target`; `Alternatives: AvailabilityAlternative[]` |
| `AvailabilityAlternative` | `Audiences: AudienceName[]` that must all be held; `Conditions: string[]` that must all hold, as the model wrote them |
| `ContextValueDeclaration` | `Name`; `Type`; `Origin`; `Scope` as a dotted path, empty for the whole application; `Required` |
| `PersonaDeclaration` | `Name`; `Audiences: AudienceName[]` |

- A gate or a block opens when any one of its alternatives does, so the operator needs no expression evaluator. An alternative with no audiences and no conditions always opens.
- Conditions are text. The operator shows them and never evaluates them; the product's own engine is the only evaluator.
- Text the model does not give is an empty string, never null. Declarations are in a fixed order, so declaring the same things in another order is not a change.
- `Version` is the product build's informational version. It is not part of what is compared, so a new build that declares the same is not reported again.
- `EveryDeclaredAudienceGranted` is true when the environment grants every declared audience to everyone, whatever the operator decided. It is the engine's environment switch, which only a local or test environment may turn on.

`AudienceDecisionReceived` is the one fact the operator cannot know for itself:

- It is published once per decision the product recorded. A decision that is delivered again produces none, and neither does one the product ignored, for instance for a tenant it does not have, so that decision stays waiting.
- `DecidedAt` is the operator decision's own occurred time. It is the only way to pair a receipt with the decision it answers.
- `Decision` is `Granted = 0` or `Withdrawn = 1`. The `Level`, `TenantId` and `UserId` follow the same table as above.
- It is not a heartbeat. Nothing is published while nothing is decided, and no event says the product is connected.

How the product reports them is in [Report what the model declares](./report-what-the-model-declares.md).

## Shared types

| Type | Shape |
| --- | --- |
| `Product` | Text; the product the decision is for |
| `TenantId` | Text; the name of the tenant's Chronicle namespace in the product |
| `UserId` | Text; the user as the product's identity provider names them |
| `AudienceName` | Text; starts with a letter, then letters, digits and underscores, up to 64 characters, case-sensitive |
| `AudienceLevel` | `Tenant = 0`, `User = 1`, `System = 2`, `NamedSystem = 3` |
| `SystemClientId` | Text; the client id a system authenticates with, up to 256 characters; empty unless the level is `NamedSystem` |
| `SystemKind` | `None = 0`, `Mcp = 1`, `Api = 2`, `Other = 3`; `None` unless the level is `NamedSystem` |
| `EnvironmentName` | Text; the environment the product reports from, such as `Production`; empty when the product does not name it |
| `ModelVersion` | Text; the product build's informational version |
| `AudienceDecision` | `Granted = 0`, `Withdrawn = 1` |

## Audience is also a routing label

Chronicle has its own routing option called the audience: a label on a subscription and on an event that decides which subscriber receives it, see [Audience routing](../BoxOffice/audience-routing.md). It is separate from the `Audience` field of these events, which is the name of the audience being granted. GuestList uses the routing audience to address a decision to its product, with the product's name as the label. The field never takes part in routing.

## Systems that call the product

The product publishes `SystemsDeclared` to say which systems call it, so an operator can pick one to grant an audience to. It is generation 1 and additive, in `Cratis.Showrunner.GuestList.Contracts.Availability`.

| Event | Fields | Meaning |
| --- | --- | --- |
| `SystemsDeclared` | `Product`; `Environment: EnvironmentName`; `Systems: SystemDeclaration[]` | The whole list of systems the product knows in one environment, reported once per change |

| Type | Fields |
| --- | --- |
| `SystemDeclaration` | `ClientId: SystemClientId`; `Kind: SystemKind`; `Name`, empty when the product gives none |

Each report replaces the earlier list of its environment, ordered by client id. A product that has never reported and knows no systems publishes nothing. How a product says which systems it knows is in [Report what the model declares](./report-what-the-model-declares.md).

## Released generations

`EventSchemas.snapshot.json` records the schema of every event. A specification compares the code with it, so a changed schema fails the build until the snapshot is regenerated on purpose:

```shell
GUESTLIST_UPDATE_EVENT_SCHEMA_SNAPSHOT=1 dotnet test Source/DotNET/GuestList.Contracts/GuestList.Contracts.csproj
```

`ReleasedEventSchemas.json` lists the generations that have shipped. A listed generation must stay in the snapshot and in code, and its schema can never change: a released event needs a new generation and a migration instead.

### Naming a system

`AudienceGranted`, `AudienceWithdrawn` and `AudienceDecisionReceived` are generation 2. Generation 1 had no client id, and its schema is kept in code (`AudienceGrantedV1` and its siblings) and in the snapshot. Chronicle reads both through a migration:

- An event of generation 1, stored or published, reads as generation 2 with an empty `ClientId` and `Kind.None`. A `System` grant that was decided before systems were named therefore stays the system-wide holder, and nothing that shipped changes meaning.
- A generation 2 event read as generation 1 loses `ClientId` and `Kind`. A `NamedSystem` decision becomes a tenant decision with no tenant, which a generation 1 product ignores as malformed, so a product that has not been updated never widens one system's grant to every system.

Update products before granting an audience to a named system, because a product that has not been updated ignores those decisions.
