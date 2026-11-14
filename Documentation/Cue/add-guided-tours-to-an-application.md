---
title: Add guided tours to an application
description: A tutorial that adds the Cue backend and React packages to an application, plays a first narrated tour and points a button at it until the person has taken it.
---

In this tutorial you add Cue to an application called `ledger`: the backend package that remembers who has taken a tour, the provider that connects the voice-over, and one tour with a button that points at it. The application must already be an Arc application with a Chronicle event store and a React frontend that uses Cratis Components.

## 1. Add the backend

Reference `Cratis.Showrunner.Cue` and call `AddCue` next to `AddCratis`, naming the tours the application has:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddCratis();
builder.AddCue(cue => cue.Tours = ["order-desk", "month-end"]);
```

`Tours` is the list of tour names the application accepts. Starting a tour that is not on it is refused, so a client cannot fill a person's history with tours that do not exist. Left empty, every name is accepted. The options are described in [Remember who has taken a tour](./remember-who-has-taken-a-tour.md#configure-the-tours).

## 2. Install the packages

```shell
yarn add @cratis/showrunner.cue.react @cratis/showrunner.cue @cratis/arc @cratis/arc.react @cratis/components @cratis/fundamentals
```

`@cratis/arc`, `@cratis/arc.react`, `@cratis/components` and `@cratis/fundamentals` are peer dependencies, so the application decides their versions. Use the package version that matches the NuGet package; everything in Showrunner is released together with one version.

## 3. Import the stylesheet

Import it once, after the Cratis Components stylesheets:

```css
@import '@cratis/components/tokens';
@import '@cratis/components/styles';
@import '@cratis/components/theme';
@import '@cratis/showrunner.cue.react/styles';
```

Every color comes from the `--cratis-*` tokens, so your theme and dark mode apply. Two properties let the tour fit your shell: `--cue-viewport-top` is how far below the top of the window the tour's layer starts, for an application with a bar of its own, and `--cue-z-index` is the layer it sits on. They default to `0` and `9999`.

## 4. Mount the provider once

Put `CueProvider` inside `<Arc>`, near the root, and give it the voice. Without a `speech` the tours play silently:

```tsx
import { Arc } from '@cratis/arc.react';
import { CratisComponentsProvider } from '@cratis/components';
import { CueProvider, createHttpSpeech } from '@cratis/showrunner.cue.react';
import { ReactNode } from 'react';

// Created once, outside any component.
const speech = createHttpSpeech({
    speakUrl: '/api/speech',
    availableUrl: '/api/speech/available'
});

export const AppShell = ({ children }: { children: ReactNode }) => (
    <CratisComponentsProvider>
        <Arc>
            <CueProvider speech={speech}>
                {children}
            </CueProvider>
        </Arc>
    </CratisComponentsProvider>
);
```

`createHttpSpeech` connects a text-to-speech service that is reached over HTTP. Create it once, outside any component. [Configure the voice-over](./configure-the-voice-over.md) describes it, and what to do when your service does not look like that.

## 5. Play a tour

A tour is a script, a stage and a way to start it. `TourOverlayShell` is the overlay: it runs the playback, draws the chapter list, the subtitles and the transport, and calls your function for the stage. `useTourNudge` says whether to point at the button and records the start:

```tsx
import { TourOverlayShell, TourSection, useTourNudge } from '@cratis/showrunner.cue.react';
import { useState } from 'react';

const orderDeskTour: TourSection[] = [
    {
        id: 'welcome',
        title: 'Welcome',
        chapters: [
            {
                id: 'the-desk',
                title: 'The desk',
                summary: 'What the order desk is for.',
                steps: [
                    { narration: 'This is the order desk. Everything a customer asks for lands here first.', stage: 0 },
                    { narration: 'Each order is a card. The newest sits on top.', stage: 1 }
                ]
            }
        ]
    }
];

const writtenChapters = {
    'the-desk': 'The order desk is where every order starts. Open a card to read the order.'
};

export const OrderDeskTour = () => {
    const [isPlaying, setIsPlaying] = useState(false);
    const { isNudgeShown, recordStart } = useTourNudge('order-desk');

    const start = () => {
        recordStart();
        setIsPlaying(true);
    };

    return (
        <>
            <button type='button' onClick={start}>
                {isNudgeShown ? 'Start here: take the tour' : 'Take the tour'}
            </button>
            {isPlaying && (
                <TourOverlayShell sections={orderDeskTour} selfPacedContent={writtenChapters} onClose={() => setIsPlaying(false)}>
                    {playback => <p>The desk has {playback.step?.stage ?? 0} cards on it.</p>}
                </TourOverlayShell>
            )}
        </>
    );
};
```

Press the button and the tour opens full screen. Escape, or the close button in the transport, hands the surface back. The first time a person presses it, the start is recorded; from then on, on every device, `isNudgeShown` is false and the button stops pointing.

`useTourNudge` shows the pointer only once what the person started has arrived, so a button never points and then withdraws the pointer. When the application knows something else that makes the tour pointless, such as the person having already done the work it teaches, pass it as the second argument, `useTourNudge('order-desk', hasDoneTheWork)`.

## 6. See it work

- The tour opens with the first line spoken and shown as a subtitle, and the chapter list on the right.
- Skipping to a chapter silences the line being said and speaks the first line of the new one.
- Pressing the voice button when the voice failed tries it again; the tour never stops for a lost voice.
- Where the tour has written chapters, the transport offers to read it at your own pace.

## Texts

The tour's own buttons are English by default. [Write a tour](./write-a-tour.md#texts-and-guards) shows how to give your own texts and how to guard the stage with your own error boundary.
