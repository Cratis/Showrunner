---
title: Playbill
description: "Show an application's release notes with Showrunner Playbill: captured when it ships, embedded in the build and drawn by styleable React components."
---
Playbill is the release-notes part of Showrunner. A release is described once, in the pull request that ships it. Playbill carries that description the rest of the way: it is committed next to the code, embedded in the assembly when the application is built, served from a fixed address, and drawn in the application as a releases page, a "what's new" dialog and an unread badge.

Playbill is a set of libraries, not a service. The backend runs in the Arc and Chronicle process of the application, the way [Marquee](../Marquee/index.md) and [Cue](../Cue/index.md) do, and the frontend is a React package built on Cratis Components:

- `Cratis.Showrunner.Playbill` embeds the release history at build time, serves it under `/.cratis/playbill/v1`, and records which releases each person has seen. See [Add release notes to an application](./add-release-notes-to-an-application.md).
- `@cratis/showrunner.playbill` holds the generated TypeScript proxies for the commands and the observable query.
- `@cratis/showrunner.playbill.react` holds the components and hooks. See [Customize the release-notes experience](./customize-the-release-notes-experience.md).
- The `playbill-capture` GitHub Action writes a release's notes into the project when it ships. See [Capture release notes when you ship](./capture-release-notes.md).

Marquee and Playbill are different things. Marquee is what the people running a product choose to tell its users, with an audience and a window. Playbill is the record of what shipped, written by the release itself.

## Guides

- [How Playbill works](./how-playbill-works.md): capture, embedding, the endpoints, the record of what people have seen and the components.
- [Add release notes to an application](./add-release-notes-to-an-application.md): the package, the endpoints, the embedded folder and a first page.
- [Capture release notes when you ship](./capture-release-notes.md): the action, its two modes and its options, and how to move over from a hand-built setup.
- [Endpoint reference](./endpoint-reference.md): the four endpoints under `/.cratis/playbill/v1`, what they return and how they are cached.
- [Customize the release-notes experience](./customize-the-release-notes-experience.md): theming, replacing a part, texts, the hooks and your own endpoint.
- [Remember what people have seen](./remember-what-people-have-seen.md): the command, the query, the options and what an unread release is.
- [Event reference](./event-reference.md): the event, what it carries and why it is personal data.

Playbill does not depend on any other Showrunner part. It does not write the notes: the pull request description does, and the repository's own release-note rules decide what it may contain.
