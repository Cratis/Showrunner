---
title: How Cue works
description: How a Cue tour is described as a script, played against one clock, spoken through a configured voice and remembered per person.
---

A guided tour has three parts that Cue keeps apart: the script the application writes, the player Cue provides, and the stage the application draws. A fourth, the voice, is plugged in.

## The script

A script is a list of sections. A section groups chapters, a chapter has a title and a one-line summary, and a chapter is a list of steps. A step is one beat of the tour:

- `narration` is the line the voice says and the subtitles show.
- `stage` says how much of the stage is built by that point. Steps are cumulative: a step never takes anything away.
- `camera`, `gesture` and `callout` say where the camera looks, where the pointer goes and what label is pinned to the stage while the step plays.
- `durationMs` is only for steps that are not narrated. A narrated step is timed from its narration, so rewording a line does not mean keeping a second number in step.

[Write a tour](./write-a-tour.md) describes every field.

## The clock

The player lays the whole script on one clock. Subtitles, the progress bar, the highlighted chapter and how much of the stage is showing are all read from that clock, so jumping to any chapter is consistent by construction.

Pacing follows the schedule, with two adjustments where the schedule and the voice disagree. A line that finishes before its step is due skips the dead air that is left. A line still being said when its step ends holds the clock at the end of the step until the line finishes, for at most 15 seconds, so a voice that never finishes cannot stall the tour.

## The voice

The player asks a configured voice to speak the line of the current step, and to prepare the next two lines while it does, so each starts the moment its step does. It watches the voice to learn when a line has finished. If the voice fails, the tour carries on with its subtitles and the voice button offers to try again. A voice that is not available at all is reported as such. [Configure the voice-over](./configure-the-voice-over.md) shows how an application provides one.

## The stage

The application draws what the tour plays over: a board, a page, a form. It gives the overlay a function that receives the playback and returns the stage, and reads from the playback what it needs: the current step, its `stage` value, the camera, the time. Cue does not know what the stage is. A stage that cannot be drawn is guarded on its own, so the transport and the chapter list stay up and the viewer can still close the tour or pick another chapter.

## Written chapters

A tour can also carry a written version of each chapter, as Markdown, keyed by chapter id. The transport then offers a switch to read the tour at your own pace: the clock and the voice stop, and the current chapter's text shows beside the chapter list. Markdown is rendered as elements, never as HTML. Raw HTML is dropped, links open in a new tab, and an address that would run script is refused.

## Who has taken a tour

The backend records the first time each person starts a tour as an event under their own identity, so it holds on every device, and an observable query tells every open page of theirs when one is added. The `useTourNudge` hook turns that into a pointer that says "start here" until the person has started the tour, and `StartTour` records the start. Starting a tour twice changes nothing. See [Remember who has taken a tour](./remember-who-has-taken-a-tour.md).
