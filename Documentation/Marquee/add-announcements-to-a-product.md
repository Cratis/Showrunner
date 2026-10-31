---
title: Add announcements to a product
description: A tutorial that adds the Marquee backend package and the Marquee React component to a product, so it shows the announcements written for it.
---

In this tutorial you add Marquee to a product called `ledger`: the backend package that receives announcements, and one component in the application shell that shows them. You end with banners and dialogs that need no other code. The product must already be an Arc application with a Chronicle event store and a React frontend that uses Cratis Components; the announcements themselves are written in an authoring application, see [Author announcements without Direct](./author-without-direct.md).

## 1. Add the backend

Reference `Cratis.Showrunner.Marquee` and call `AddMarquee` next to `AddCratis`:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddCratis();
builder.AddMarquee(marquee =>
{
    marquee.Product = "ledger";
    marquee.AuthoringStores = ["Direct"];
});
```

`Product` is the name announcements are written for, and the host does not start without it. `AuthoringStores` names the event store the announcements are published from; it defaults to `Direct`. The options are described in [Receiving](./receiving.md#add-marquee-to-a-product).

The component brings the generated query and command proxies from `@cratis/showrunner.marquee`; you do not import them yourself.

## 2. Install the component

```shell
yarn add @cratis/showrunner.marquee.react @cratis/showrunner.marquee @cratis/arc @cratis/arc.react @cratis/components @cratis/fundamentals
```

`@cratis/arc`, `@cratis/arc.react`, `@cratis/components` and `@cratis/fundamentals` are peer dependencies, so the application decides their versions. Use the package version that matches the NuGet package; everything in Showrunner is released together with one version.

## 3. Import the stylesheet

Import it once, after the Cratis Components stylesheets:

```css
@import '@cratis/components/tokens';
@import '@cratis/components/styles';
@import '@cratis/components/theme';
@import '@cratis/showrunner.marquee.react/styles';
```

Every color comes from the `--cratis-*` tokens, so your theme and dark mode apply.

## 4. Mount `<Marquee />` once

Put it first in the shell, inside `<Arc>` and a `CratisComponentsProvider`:

```tsx
import { Arc } from '@cratis/arc.react';
import { CratisComponentsProvider } from '@cratis/components';
import { Marquee } from '@cratis/showrunner.marquee.react';
import { ReactNode } from 'react';

export const AppShell = ({ children }: { children: ReactNode }) => (
    <CratisComponentsProvider>
        <Arc>
            <Marquee />
            {children}
        </Arc>
    </CratisComponentsProvider>
);
```

That is all. The banners sit at the top of the page, above everything the shell draws after them, and stick to the top while the page scrolls. A dialog opens over the application, one at a time.

## 5. See it work

Author an announcement for `ledger` and:

- A **banner** appears at the top within moments. It has no close button; it goes away when it is withdrawn or its window ends.
- A **dialog** opens, the most important first. Its button says *Got it*, or *Next* while more follow.
- Close a `DialogOnce` and it is gone for that person on every device; close a `DialogEverySession` and it returns in the next session.
- An announcement scheduled for later appears by itself within a minute of its start.

## Texts and timing

Every text the component shows is a prop, so you can localize it, and the clock check can be changed:

```tsx
import { Marquee } from '@cratis/showrunner.marquee.react';

export const LocalizedMarquee = () => (
    <Marquee
        checkInterval={30_000}
        labels={{
            banners: 'Kunngjøringer',
            acknowledge: 'Forstått',
            next: 'Neste',
            close: 'Lukk',
            information: 'Informasjon',
            warning: 'Advarsel',
            critical: 'Kritisk'
        }} />
);
```

Labels you leave out stay English.

## Accessibility

- The banner region is a polite live region, named by `labels.banners`, and is always in the page, so a banner that arrives is announced.
- A dialog is the Cratis Components dialog: it keeps focus inside until it is closed and closes with Escape. Focus lands on the title, not on the button, so the Enter or Space that was being pressed when the dialog opened does not close it unread.
- The importance icon is named, so a screen reader hears *Warning* with the title.

## Next

- Draw the banner or the dialog yourself: [Replace the banner or the dialog](./replace-the-banner-or-the-dialog.md).
- How the pieces fit: [How Marquee works](./how-marquee-works.md).
