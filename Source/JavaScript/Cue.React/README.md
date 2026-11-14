# @cratis/showrunner.cue.react

The Cue guided-tour player for React, the Showrunner guided-tour part. A tour is a script of sections, chapters and steps;
the player lays it on one clock and plays it with a voice-over, subtitles, a chapter list, a pointer and callouts, and a
written version of each chapter. Built on `@cratis/components` and the generated proxies in `@cratis/showrunner.cue`.
How to use it: [Documentation/Cue](../../../Documentation/Cue/index.md).

## Component hierarchy

```
CueProvider                 speech, labels, stage guard for every tour below it
└── TourOverlayShell        useTourPlayback, then the chrome and the application's stage
    ├── <stage>             drawn by the application, from the playback, inside the stage guard
    ├── TourSelfPacedPanel  written chapter, in self-paced mode
    │   └── CueMarkdown
    ├── TourChapterList
    ├── TourSubtitles
    └── TourTimelineBar     transport: chapters, play, scrubber, voice, subtitles, self-paced, close
```

`TourAnchor`, `TourPointer` and `TourCalloutBubble` are for the application's stage: they hold the pointer and the
callouts at a place on a board and follow the camera.

## Architecture decisions

- **The script is data, the clock is the single source of truth.** `Tour*.ts` types describe the script; `tourTimeline.ts`
  lays it on a clock; `tourClock.ts` decides how the clock advances against the voice. Subtitles, progress, the current
  chapter and how much of the stage shows are all read from the clock.

  | Module | Decides |
  | --- | --- |
  | `tourTimeline.ts` | where each step, chapter and section sits on the clock, and what comes next or before |
  | `tourClock.ts` | how far the clock moves in a tick, when a line ends a step early and when it holds one |
  | `tourSelfPaced.ts` | whether there is anything to read, which chapter's text, and the media addresses in it |
  | `tourGeometry.ts` | where a point sits on screen, in each of the four spaces |
  | `Speech/speechClips.ts` | which lines have been asked of the service, and which are kept |
  | `Markdown/safeUrl.ts` | which addresses a chapter's Markdown may point at |

- **The voice is configured, never chosen.** The player talks to a `CueSpeech`; where the audio comes from is the
  application's `CueSpeechConfiguration` on `CueProvider`. `createHttpSpeech` is one for a service reached over HTTP.
  Without one a tour plays silently.
- **The stage belongs to the application.** The overlay calls a function for it and guards it on its own, so the
  transport stays up when it fails.
- **Markdown is rendered as elements, never as HTML.** `react-markdown` with `skipHtml`; every address goes through
  `safeUrl`.
- **Per-frame work stays out of React.** `TourAnchor` writes its position straight to the DOM from a self-suspending
  frame loop; the clock ticks every 100 ms.
- **No MVVM, no UI framework beyond Cratis Components' tokens.** Every color comes from the `--cratis-*` tokens, and the
  transport's icons are inline.

## Examples and stories

`Examples/` holds the code shown in the documentation. It is compiled with the package, and a specification fails when a
documentation page no longer contains an example exactly as it builds. `yarn storybook` runs the stories of the chapter
list, the transport and the written-chapter panel, against a small made-up tour.
