---
title: Contracts
description: The public events Box Office exchanges between an operator such as Direct and a managed product, their payloads, and how each replaces a v1 Admin contract.
---

`Cratis.Showrunner.BoxOffice.Contracts` holds the public events an operator and a managed product exchange about tenants. The operator, such as Direct, decides what a tenant may do and publishes it. The product honors it and publishes what happens in it. Both sides reference this package.

## Rules every contract follows

- **The organization is the event source.** Every tenant-scoped event, in both directions, is appended under the organization's `OrganizationId`: a GUID in its lowercase, hyphenated `D` form. It is the operator's own identity for the organization, and it is event context, not a field in the payload. The product answers under the same id it received.
- **Product-wide events have their own event source.** `OfferingDeclared` is appended under the product's `ProductId`. `OfferPublished` and `OfferWithdrawn` are appended under the offer's `OfferId`. Neither id is in a payload.
- **Every tenant-scoped payload carries `TenantId`.** It is the name of the tenant's Chronicle namespace in the product. In Direct it is the lobby's tenant name. The operator serves many tenants, and the product appends each event in the namespace `TenantId` names. The three product-wide events have no tenant.
- **Nothing is nullable.** An optional collection is empty, and an unknown value is the concept's `NotSet`. An entitlement without an end date uses `EntitlementEnd.Never`.
- **No timestamps that repeat the event's own time.** When something happened is the event's `Occurred`. The only dates in payloads are business dates: a trial's last day, an entitlement's last day, and the metered or summarized day.
- **Every event is `[Public]`, and none names an origin store.** No contract shares a name with a private event. Direct's private events keep their `Customer…` names. Both sides subscribe explicitly to the store they read from.
- **The schemas are guarded.** `EventSchemas.snapshot.json` holds every event's schema, and `ReleasedEventSchemas.json` lists the generations that are released and so can never change. Changing a released event takes a new generation and a migration.

Namespaces are relative to `Cratis.Showrunner.BoxOffice.Contracts`. Concepts are `ConceptAs<string>` unless noted.

## Operator to product

| Event | Namespace | Payload |
| --- | --- | --- |
| `TenantRegistered` | `Tenants` | `TenantId`; `Name: TenantName`; `Creator: Person`; `CreatorRole: TenantRole` |
| `TrialOpened` | `Trials` | `TenantId`; `Ends: TrialEnd`; `Seats: SeatCount` |
| `TrialEndMoved` | `Trials` | `TenantId`; `Ends: TrialEnd` |
| `TrialClosed` | `Trials` | `TenantId`; `Closure: TrialClosure` (`Expired`, `EndedEarly`, `Converted`) |
| `SeatCapacitySet` | `Seats` | `TenantId`; `Capacity: SeatCount`; `Until: EntitlementEnd`; `Source: SeatCapacitySource` (`Grant`, `Purchase`) |
| `ModuleGranted` | `Entitlements` | `TenantId`; `Module: ModuleId`; `Until: EntitlementEnd` |
| `ModuleTrialGranted` | `Entitlements` | `TenantId`; `Module: ModuleId`; `Ends: TrialEnd` |
| `ModulePreviewOpened` | `Entitlements` | `TenantId`; `Module: ModuleId`; `Until: EntitlementEnd` |
| `ModuleRevoked` | `Entitlements` | `TenantId`; `Module: ModuleId` |
| `AllowanceSet` | `Compute` | `TenantId`; `Meter: MeterId`; `Quantity: MeterQuantity`; `Period: AllowancePeriod` (`Day`, `Month`); `Overage: OverageBehavior` (`Block`, `Park`) |
| `TenantSuspended` | `Tenants` | `TenantId`; `Cause: SuspensionCause` (`TrialEndedWithoutSeats`, `PaymentOverdue`, `AdministratorDecision`) |
| `TenantReinstated` | `Tenants` | `TenantId` |
| `TenantClosing` | `Tenants` | `TenantId` |

- `TenantRegistered` asks the product to create the tenant. The creator is a `Person`: identity provider, subject, name and email. The product answers with `TenantProvisioned`, and the operator starts the trial's clock from that, not from registering.
- `SeatCapacitySet` carries the total the tenant may now assign, not the amount added. Granted or bought seats replace the trial seats, so a tenant on a trial of 3 that is granted 5 has 5, not 8.
- A suspended tenant is read-only in the product until `TenantReinstated`. A trial that ends without seats suspends the tenant with `TrialEndedWithoutSeats`.
- `TenantClosing` tells the product to tear the tenant down.

## Operator, product-wide

These concern no single tenant. They let a product show its plan page without calling the operator.

| Event | Namespace | Event source | Payload |
| --- | --- | --- | --- |
| `OfferPublished` | `Offers` | `OfferId` | `Name: OfferName`; `Modules: IEnumerable<ModuleId>`; `IncludedSeats: SeatCount`; `Prices: IEnumerable<OfferPrice>` |
| `OfferWithdrawn` | `Offers` | `OfferId` | none |

An `OfferPrice` is an `Interval: BillingInterval` (`Monthly`, `Yearly`), an `Amount: PriceInMinorUnits` (cents, not a decimal) and a `Currency: CurrencyCode`. Publishing an offer again with the same `OfferId` changes it. A withdrawn offer is no longer sold; tenants that have it keep it.

## Product to operator

| Event | Namespace | Event source | Payload |
| --- | --- | --- | --- |
| `OfferingDeclared` | `Offering` | `ProductId` | `Modules: IEnumerable<ModuleDeclaration>`; `Meters: IEnumerable<MeterDeclaration>`; `Seats: SeatLicensing` (`NotLicensed`, `PerUser`) |
| `TenantProvisioned` | `Tenants` | `OrganizationId` | `TenantId` |
| `SeatTaken` | `Seats` | `OrganizationId` | `TenantId`; `Holder: Person` |
| `SeatReleased` | `Seats` | `OrganizationId` | `TenantId`; `Holder: Person` |
| `ActivitySummarized` | `Activity` | `OrganizationId` | `TenantId`; `Day: ActivityDay`; `ActiveUsers: ActiveUserCount`; `Sessions: SessionCount`; `SignIns: SignInCount` |
| `ComputeDayClosed` | `Compute` | `OrganizationId` | `TenantId`; `Day: ComputeDay`; `Usage: IEnumerable<MeterUsage>` |
| `ComputeDayCorrected` | `Compute` | `OrganizationId` | `TenantId`; `Day: ComputeDay`; `Usage: IEnumerable<MeterUsage>` |
| `AllowanceExhausted` | `Compute` | `OrganizationId` | `TenantId`; `Meter: MeterId`; `Day: ComputeDay`; `Overage: OverageBehavior` |

- A product publishes `OfferingDeclared` once per change to what it sells, not on every start.
- `Usage` is empty when nothing was used. A `MeterUsage` pairs a `MeterId` with a `MeterQuantity` in the meter's unit.
- `ComputeDayCorrected` replaces the usage of `ComputeDayClosed` for the same day; it carries the corrected totals, not a difference.
- `ActivitySummarized` is published once a day per tenant.

## Mapping from the v1 contracts

Box Office replaces the v1 `Admin.Contracts` events the operator sent to a product. The first table lists every v1 contract and what replaces it. The second lists Direct's own facts about seats, trials and suspension, so each maps one to one.

| v1 contract | Replacement | What changes |
| --- | --- | --- |
| `OrganizationRegisteredForTenant(Tenant)` | `TenantRegistered` | Carries the name, the creator and the creator's role. The organization is the event source, as before. |
| `TrialEndChangedForTenant(Tenant, Expiration)` | `TrialEndMoved` | Names the trial, not the seat it moved. `Ends` is a `TrialEnd`. |
| `SeatsGrantedToTenant(Tenant, Sku, Quantity, Expiration)` | `SeatCapacitySet` | Carries the resulting capacity and its source. The product no longer assigns a seat to every user. |
| `TenantDeleted(Tenant)` | `TenantClosing` | Names the fact the product acts on: the organization is being deleted, so the product tears the tenant down. |
| `AnnouncementPublishedToAllTenants`, `AnnouncementAmendedForAllTenants`, `AnnouncementWithdrawnFromAllTenants` | Not Box Office | Announcements are a separate part. |
| `TermsAndConditionsPublished`, `PrivacyPolicyPublished`, `LegalDocumentsPublished` | Not Box Office | Legal documents are not licensing. |
| `TrialOfferedForInvitation(TrialDays)` | `TrialOpened` | The trial opens when the tenant is provisioned, with its end date, not at invitation. |

| Operator fact in Direct | Box Office event |
| --- | --- |
| `CustomerOrganizationRegistered` | `TenantRegistered` |
| `CustomerOrganizationProvisioned` (the product's report) | `TenantProvisioned` |
| `CustomerTrialStarted` | `TrialOpened` |
| `CustomerTrialEndMoved` | `TrialEndMoved` |
| `CustomerTrialEnded` | `TrialClosed` (`Expired` for the automatic end, `EndedEarly` for an administrator) |
| `CustomerSeatsGranted`, `CustomerSeatsRevoked` | `SeatCapacitySet` (the resulting total) |
| `CustomerSeatAssigned` | `SeatTaken` |
| `CustomerSeatReleased` | `SeatReleased` |
| `RecordCustomerActivity` | `ActivitySummarized` |
| The product turning read-only | `TenantSuspended` and `TenantReinstated` |
| `CustomerOrganizationDeletionStarted` | `TenantClosing` |

`ProductId` and `OrganizationId` are the operator's own GUID identities, so Direct appends under the ids it already has. Direct's private events keep their `Customer…` names, so no contract here shares a name with one.
