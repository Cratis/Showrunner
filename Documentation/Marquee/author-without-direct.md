---
title: Author announcements without Direct
description: How to run the Marquee authoring backend in your own application and publish announcements to your products, without Direct.
---

Direct is one application that authors announcements. Nothing in Marquee requires it: authoring is the package `Cratis.Showrunner.Marquee.Authoring`, and any Arc application with a Chronicle event store can host it. Use this when the people running your product write announcements in a tool of your own.

## Host the authoring backend

1. Reference `Cratis.Showrunner.Marquee.Authoring` from the Arc application that will own announcements. Arc discovers its commands and query, and Chronicle its events, read model and outbox reactor.
2. Register the policy that admits who may author. Every command and the query require `MarqueeAuthoring.Policy`, and a host that does not register it can not use them:

```csharp
builder.Services.AddArcAuthorizationPolicy<OperatorsOnly>(MarqueeAuthoring.Policy);
```

3. Tell each product which event store the announcements are published from. The name of the store the authoring application runs in goes in the product's `AuthoringStores`, which defaults to `Direct`; see [Receiving](./receiving.md#add-marquee-to-a-product).

The commands, the outbox and what it guarantees are in [Authoring](./authoring.md).

## Write the screen

Install the proxies, with no UI framework dependency:

```shell
yarn add @cratis/showrunner.marquee.authoring @cratis/arc @cratis/arc.react @cratis/fundamentals
```

Author an announcement with the command. An audience is a plain object whose `kind` decides which of the other values apply; leave the rest empty. The caller chooses the id, so a retried command is refused as already authored instead of authoring a second announcement:

```ts
import { Guid } from '@cratis/fundamentals';
import { AuthorAnnouncement, Audience, AudienceKind, Display, Importance } from '@cratis/showrunner.marquee.authoring';

export const authorMaintenanceNotice = async () => {
    const audience = new Audience();
    audience.kind = AudienceKind.everyone;
    audience.tenantIds = [];
    audience.module = '';
    audience.plan = '';

    const author = new AuthorAnnouncement();
    author.announcementId = Guid.create();
    author.product = 'ledger';
    author.audience = audience;
    author.title = 'Planned maintenance';
    author.body = 'Ledger is read-only on **Saturday** from 22:00 to 23:00 UTC.';
    author.display = Display.banner;
    author.importance = Importance.warning;
    author.activeFrom = new Date('2026-11-06T00:00:00Z');
    author.activeUntil = new Date('2026-11-08T00:00:00Z');

    const result = await author.execute();
    return result.isSuccess;
};
```

List what has been written for a product, withdrawn announcements included and the newest window first. The list is sent again each time one changes:

```tsx
import { ForProduct } from '@cratis/showrunner.marquee.authoring';

export const AnnouncementList = () => {
    const [announcements] = ForProduct.use({ product: 'ledger' });

    return (
        <ul>
            {announcements.data.map(announcement => (
                <li key={announcement.id.toString()}>{announcement.title}</li>
            ))}
        </ul>
    );
};
```

`AmendAnnouncement`, `RetargetAnnouncement` and `WithdrawAnnouncement` change what was authored. How an announcement is displayed can not be changed: withdraw it and author another. A title holds up to 100 characters, a body up to 2,000, and leaving the window at its default date means *from now* and *with no end*.
