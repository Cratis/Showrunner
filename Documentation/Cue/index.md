---
title: Cue
description: Add scripted, narrated guided tours to an application with Showrunner Cue, with a pluggable text-to-speech voice-over and a durable record of who has taken which tour.
---

Cue is the guided-tour part of Showrunner. An application describes a tour as a script of sections, chapters and steps, and Cue plays it like a short video: a narrated walkthrough with subtitles, a pointer that moves to what the narration talks about, a chapter list to jump around in, and a written version of each chapter for people who would rather read.

Cue is a set of libraries, not a service. The backend runs in the Arc and Chronicle process of the application that hosts it, the way [Usher](../Usher/index.md) and [Marquee](../Marquee/index.md) do, and the frontend is a React package built on Cratis Components:

- `Cratis.Showrunner.Cue` records which tours each person has started, so the application can point at a tour once and stop. See [Remember who has taken a tour](./remember-who-has-taken-a-tour.md).
- `@cratis/showrunner.cue` holds the generated TypeScript proxies for that backend.
- `@cratis/showrunner.cue.react` holds the player: the script types, the overlay with its chapter list, subtitles and transport, the pointer and callouts, and the hooks that connect the tour to the backend.

The voice-over is the one thing Cue cannot bring. A tour speaks through a text-to-speech service that the application configures, and a tour without one plays silently with its subtitles. See [Configure the voice-over](./configure-the-voice-over.md).

## Guides

- [How Cue works](./how-cue-works.md): the script, the clock, the voice and the record of who has taken a tour.
- [Add guided tours to an application](./add-guided-tours-to-an-application.md): the backend package, the provider, a first tour and a button that points at it.
- [Write a tour](./write-a-tour.md): sections, chapters, steps, the camera, the pointer, callouts and written chapters.
- [Configure the voice-over](./configure-the-voice-over.md): connect a text-to-speech service, or bring your own voice.
- [Remember who has taken a tour](./remember-who-has-taken-a-tour.md): the command, the query, the options and the event.
- [Event reference](./event-reference.md): the event, what it carries and why it is personal data.

Cue does not depend on any other Showrunner part. It does not draw the stage a tour plays on, such as a board or a page: the application supplies that, and Cue plays the script over it.
