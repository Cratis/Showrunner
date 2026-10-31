---
title: Frontend proxies
description: The @cratis/showrunner.marquee and @cratis/showrunner.marquee.authoring packages - the generated TypeScript proxies for Marquee's commands and queries.
---

Arc generates a TypeScript proxy for every command and query. Marquee ships them in two packages with no UI framework dependency, because the people who write announcements and the people who read them use different applications.

| Package | Proxies | Backend |
| --- | --- | --- |
| `@cratis/showrunner.marquee` | `AnnouncementsForMe`, `AcknowledgeAnnouncement`, `Display` and `Importance` | `Cratis.Showrunner.Marquee` |
| `@cratis/showrunner.marquee.authoring` | `AuthorAnnouncement`, `AmendAnnouncement`, `RetargetAnnouncement`, `WithdrawAnnouncement`, `Announcement`, `Audience`, `AudienceKind`, `Display` and `Importance` | `Cratis.Showrunner.Marquee.Authoring` |

```shell
yarn add @cratis/showrunner.marquee @cratis/arc @cratis/arc.react @cratis/fundamentals
```

`@cratis/arc`, `@cratis/arc.react` and `@cratis/fundamentals` are peer dependencies, so the application decides their versions. Use the package version that matches the NuGet packages: everything in Showrunner is released together with one version.

```ts
import { AnnouncementsForMe, AcknowledgeAnnouncement } from '@cratis/showrunner.marquee';

const [announcements] = AnnouncementsForMe.use();
const acknowledge = new AcknowledgeAnnouncement();
acknowledge.announcementId = announcements.data[0].id;
await acknowledge.execute();
```

The proxies are generated from the C# code on a Debug build; never edit them. CI rebuilds in Debug and fails when the committed proxies differ.

The components that use the receiving proxies are in `@cratis/showrunner.marquee.react`; see [Add announcements to a product](./add-announcements-to-a-product.md).
