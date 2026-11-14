---
title: Write a tour
description: Describe a Cue guided tour as a script of sections, chapters and steps, with narration, camera, pointer and callouts, written chapters, and your own texts and stage guard.
---

A tour is an array of `TourSection`, and everything the player does follows from it. The script is plain data, so it can live next to the surface it tours, be type-checked and be specified.

## Sections, chapters and steps

```text
TourSection   id, title, chapters
  TourChapter id, title, summary, steps
    TourStep  narration, stage, camera?, gesture?, callout?, durationMs?
```

The `id` of a chapter is what a written chapter is keyed by, and it must be unique across the script. The title and summary appear in the chapter list.

## Narration and time

`narration` is spoken and shown as a subtitle. A narrated step takes as long as its line takes to say at an even pace, so there is no number to keep in step with the words. Give `durationMs` only to a step with nothing to say, such as a pause for the camera to settle. A step with an empty narration silences whatever was still being said.

Write narration to be heard: short sentences, no abbreviations the voice would spell out.

## The stage

`stage` says how much of the stage is built by the end of the step. The value means whatever the application wants it to: the number of cards on a board, the index of a pane. Steps are cumulative, so the stage only ever grows as the tour plays, and seeking backwards shows the stage as it was at that point.

The function you give `TourOverlayShell` receives the playback, which holds `step`, `timeMs`, `isPlaying`, `chapterIndex`, `sectionIndex` and the controls. Read the stage from it and keep the expensive part in a memoized component of its own.

## The camera, the pointer and callouts

When the stage is a pannable board, three optional fields say where to look:

- `camera` is `{ x, y, zoom }`: the board coordinate to bring to the center of what the tour's own chrome leaves open, and the zoom to settle on. Omitted, the camera stays where the previous step left it.
- `gesture` moves the pointer to a point and optionally presses there (`clicks`), or presses, glides to `dragTo` and releases.
- `callout` pins a short label above a point for as long as the step plays.

Draw them with `TourPointer` and `TourCalloutBubble`, which follow the camera through the board's live `transformRef`. A point is expressed in one of four spaces, set by the `space` field of the gesture or the callout:

| Space | Meaning |
| --- | --- |
| `world` | Board coordinates. The point stays glued to the board as the camera moves. The default. |
| `screen` | Pixels from the top left of the board's box. The point ignores the camera, as the application's own chrome does. |
| `editor` | Pixels from the center of a panel pinned on the middle of the space the tour's chrome leaves open. |
| `toolbar` | Pixels from the left edge across and from the vertical center down. |

## Written chapters

Pass `selfPacedContent` to `TourOverlayShell`, a record from chapter id to Markdown, and the transport offers to read the tour at your own pace. Chapters left out show nothing. A chapter's Markdown can refer to images and animations by a path relative to its own folder, such as `media/desk.png`; `resolveTourMedia(markdown, mediaUrls)` swaps each reference for the address the bundler gave the file, and leaves a reference it does not know as it was.

## Texts and guards

The tour's own buttons are English by default. Give the texts you want on the provider; the ones left out stay English:

```tsx
import { CueProvider } from '@cratis/showrunner.cue.react';
import { ReactNode } from 'react';

export const LocalizedCue = ({ children }: { children: ReactNode }) => (
    <CueProvider
        labels={{
            close: 'Lukk omvisningen',
            play: 'Spill av',
            pause: 'Pause',
            previousChapter: 'Forrige kapittel',
            nextChapter: 'Neste kapittel',
            chapters: 'Kapitler'
        }}>
        {children}
    </CueProvider>
);
```

The narration and the titles are your script's own words and are translated by writing the script in the language you want.

A tour that cannot draw its stage must not take the transport with it. Cue guards the stage on its own with a small panel and a Try again button. To report the failure the way the rest of the application does, give the provider a guard of your own:

```tsx
import { CueProvider } from '@cratis/showrunner.cue.react';
import { Component, ReactNode } from 'react';

class ReportingGuard extends Component<{ children: ReactNode }, { failed: boolean }> {
    state = { failed: false };

    static getDerivedStateFromError() {
        return { failed: true };
    }

    componentDidCatch(error: Error) {
        console.error('The tour could not be drawn.', error);
    }

    render() {
        return this.state.failed ? <p>The tour could not be drawn.</p> : this.props.children;
    }
}

export const GuardedCue = ({ children }: { children: ReactNode }) => (
    <CueProvider stageBoundary={ReportingGuard}>{children}</CueProvider>
);
```
