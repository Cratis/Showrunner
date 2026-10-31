---
title: React components
description: The @cratis/showrunner.boxoffice.react package - slot Box Office into a React application with BoxOfficeProvider, hide navigation and pages of modules the tenant lacks, show trial and suspension banners, send people to the customer portal, and render the seats, usage and plan pages.
---

`@cratis/showrunner.boxoffice.react` is the user interface of Box Office. It tells the signed-in user what their tenant may do, keeps the product from showing what the tenant has no module for, and gives the product's maintainers a seats page, a usage page and a plan page. It is built on `@cratis/components` and the generated [frontend proxies](./frontend-proxies.md), so every dialog and table is a Cratis Components one bound to Box Office's commands and queries.

The backend refuses what a tenant may not do whatever the frontend shows; see [Module gating](./gating.md), [Suspension](./suspension.md) and [Seat licensing](./seats.md). The components are what keep a user from meeting those refusals.

## Slot Box Office into an application

Box Office needs the backend, the provider and, where they fit, the banners and pages.

**Backend.** Reference `Cratis.Showrunner.BoxOffice` and call `AddBoxOffice`; see [Receiving and entitlements](./entitlements.md#add-box-office-to-a-product). To let people buy, try and change plan, set the customer portal's address and signing key as well; see [The customer portal](./portal.md).

**Frontend.** Install the package and its peers:

```shell
yarn add @cratis/showrunner.boxoffice.react @cratis/showrunner.boxoffice @cratis/arc @cratis/arc.react @cratis/components @cratis/fundamentals
```

Import the stylesheet once, after the Cratis Components stylesheets:

```css
@import '@cratis/components/tokens';
@import '@cratis/components/styles';
@import '@cratis/components/theme';
@import '@cratis/showrunner.boxoffice.react/styles';
```

Wrap the application in `BoxOfficeProvider`, inside `<Arc>`, the dialog registration and a `CratisComponentsProvider` with a toaster:

```tsx
import { Arc } from '@cratis/arc.react';
import { DialogComponents } from '@cratis/arc.react/dialogs';
import { CratisComponentsProvider } from '@cratis/components';
import { BusyIndicatorDialog, ConfirmationDialog } from '@cratis/components/Dialogs';
import { BoxOfficeProvider, NoLicense, SuspensionBanner, TrialBanner } from '@cratis/showrunner.boxoffice.react';

export const App = () => (
    <CratisComponentsProvider toaster>
        <Arc>
            <DialogComponents confirmation={ConfirmationDialog} busyIndicator={BusyIndicatorDialog}>
                <BoxOfficeProvider>
                    <TrialBanner />
                    <SuspensionBanner />
                    <NoLicense />
                    <Application />
                </BoxOfficeProvider>
            </DialogComponents>
        </Arc>
    </CratisComponentsProvider>
);
```

`BoxOfficeProvider` asks the backend what the tenant may do with the `CurrentEntitlements` query and where the user stands with the seats with `Mine`, once, and shares the answer with everything below it.

## BoxOfficeProvider

| Prop | Default | Meaning |
| --- | --- | --- |
| `labels` | English | The texts to replace; see [Texts and localization](#texts-and-localization). |
| `locale` | the browser's | The locale dates, money and quantities are formatted for. |
| `openLink` | opens a new tab | What to do with a link to the customer portal. Replace it to open the portal in the same window or inside a desktop shell. |
| `entitlements`, `seat`, `portalLink` | none | Fixed answers instead of asking the backend. For specifications and previews; leave them out in the application. |

## What the tenant may do

`useEntitlements` answers from the one place the provider asked:

```tsx
import { useEntitlements } from '@cratis/showrunner.boxoffice.react';

const entitlements = useEntitlements();

entitlements.state;            // 'loading' | 'suspended' | 'trialClosed' | 'trial' | 'licensed' | 'unlicensed'
entitlements.isReadOnly;       // can read, cannot change
entitlements.seats;            // seats the tenant has today
entitlements.trial?.daysLeft;  // 0 on the last day, negative after it
entitlements.has('analytics'); // module in full or on trial; a preview does not count
```

| Property | Meaning |
| --- | --- |
| `isLoading` | Nothing is known yet. Everything that belongs to a module is hidden until it is, so nothing flashes up and is taken away. |
| `state` | Where the tenant stands with its license. A suspension and a closed trial come before the rest, because they hold for everyone in the tenant. |
| `readOnlyReason` | Why the tenant is read-only, as in the `TenantStanding` of [Suspension](./suspension.md). |
| `trial` | The last day of the trial, the days left and whether it has closed. Present when the tenant is or was on a trial. |
| `modules`, `has`, `accessTo`, `standingOf` | What the tenant may do with each module the product declares: none, preview, trial or full, and until when. |
| `canOpenPortal` | Whether the product has a [customer portal](./portal.md). |

The backend decides a module's access from the clock, so a trial that ended is `none` the next day without anything having been recorded.

## Hide what belongs to a module the tenant lacks

Give each navigation entry or page the `module` that owns it and filter:

```tsx
import { useModuleFilter } from '@cratis/showrunner.boxoffice.react';

const entries = [
    { path: '/', title: 'Home' },
    { path: '/reports', title: 'Reports', module: 'analytics' },
];

const visible = useModuleFilter(entries);
```

An entry with no `module` belongs to the product and always shows. `filterByModule(items, entitlements)` is the same without a hook.

To guard content on a page, wrap it in `RequiresModule`:

```tsx
import { RequiresModule } from '@cratis/showrunner.boxoffice.react';

<RequiresModule module='analytics'>
    <Reports />
</RequiresModule>

<RequiresModule module='analytics' mode='explain'>
    <Reports />
</RequiresModule>
```

By default the content is hidden, or replaced by `fallback`. In `explain` mode the user is told the module is not part of their plan and offered to try or buy it, as far as the product has a customer portal.

## Banners

| Component | Shows |
| --- | --- |
| `TrialBanner` | How many days the trial has left, as a warning in its last week and as an error once it has closed, with a button to buy. Nothing for a tenant that is not on a trial. |
| `SuspensionBanner` | That the operator suspended the tenant, the cause the operator gave, and that everything is read-only until it is lifted. Asks `CurrentStanding` for the cause only while the tenant is suspended. |
| `NoLicense` | Why a user cannot use the product: the tenant is suspended, its trial closed, the user holds no seat, or the tenant holds more seats than it has bought. Nothing to a user with a license. |

Render them once, above the pages. `SuspensionBannerView` is the banner without the query, for a page that already has the cause.

## The customer portal

`PortalButton` opens the operator's customer portal with a link the backend signs, for what the user wants to do:

```tsx
import { PortalButton } from '@cratis/showrunner.boxoffice.react';
import { PortalIntent } from '@cratis/showrunner.boxoffice';

<PortalButton intent={PortalIntent.buy} label='Buy more seats' />
<PortalButton intent={PortalIntent.try} label='Try Analytics' />
<PortalButton intent={PortalIntent.changePlan} offer={offer.id} label='Change plan' />
```

It is not rendered when the product has no customer portal, so a button never leads nowhere. A failure to get a link is a toast, not an exception. How the link is made and what the portal can rely on is in [The customer portal](./portal.md).

## Pages

The three pages are for the product's maintainers, the people the backend lets manage seats and compute. Mount them on routes of your own; each is a plain component and works in any router:

```tsx
import { PlanPage, SeatsPage, UsagePage } from '@cratis/showrunner.boxoffice.react';

<Route path='/settings/seats' element={<SeatsPage />} />
<Route path='/settings/usage' element={<UsagePage />} />
<Route path='/settings/plan' element={<PlanPage />} />
```

| Page | What it does |
| --- | --- |
| `SeatsPage` | Lists who holds a seat, oldest first, marking the ones the tenant's seats do not cover. Assigns a seat with a dialog, and releases one after a confirmation. Both are disabled while the tenant is read-only. Reads `CurrentRoster`, so it needs the `BoxOffice.ManageSeats` policy; see [Seat licensing](./seats.md). |
| `UsagePage` | One row per meter the product declares: what is used against the allowance in the period it is in, when the period starts over, what happens beyond the allowance, the budget and, for a meter that has one, the concurrency limit. Reads `CurrentUsage`, so it needs the `BoxOffice.ManageCompute` policy; see [Compute admission](./compute-admission.md). |
| `PlanPage` | Where the tenant stands, every module and its access, and the offers on sale with their prices, each with a button to buy, try or change plan. Open to every signed-in user. |

`SeatsView`, `UsageView` and `PlanView` are the same pages without their data, for an application that fetches it elsewhere.

## Accessibility

- Every button has an accessible name, and a row's release button names the person it releases.
- The usage bars are progress bars named after their meter, and a meter that has used its allowance or budget says so in text as well as color.
- The assign dialog opens with focus in its content, not on **Assign**, so a held Enter key does not give a seat nobody chose.
- Banners are Cratis Components messages, announced to assistive technology when they appear.

## Texts and localization

Every text Box Office shows or announces comes from `labels`. Pass only the texts you replace; the rest keep their English defaults:

```tsx
<BoxOfficeProvider labels={{
    trialBanner: { running: 'Prøveperioden din slutter om {days} dager.', buy: 'Kjøp' },
    seats: { title: 'Seter' }
}}>
```

The full set is the `BoxOfficeLabels` type, and `defaultBoxOfficeLabels` holds the English texts. Placeholders in braces, such as `{days}` and `{held}`, are filled in by the components.

## Without the components

The models behind the components are exported: `entitlementsOf`, `trialBannerModel`, `noLicenseModel`, `seatRows`, `meterUsageModel` and `moduleRows`. They are pure functions of what the backend said, so a product with its own design can use the decisions and bring its own markup.
