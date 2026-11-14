// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { MINIMUM_STEP_MS, type TourTimeline, stepIndexAtTime } from './tourTimeline.js';

/**
 * The breath between a narration finishing and the clock moving on. Step lengths are estimated from word
 * count, and when the voice finishes ahead of the estimate the remainder would be dead air — the clock
 * skips it after this short beat rather than sitting the estimate out. The same beat separates a line that
 * ran past its estimate from the line that follows it, so held steps release calmly instead of abruptly.
 */
export const NARRATION_ADVANCE_BEAT_MS = 700;

/**
 * How long the clock will wait at a step's end for its line to finish being said. Speech is synthesized and
 * streamed, so a line regularly outlasts its word-count estimate — the clock holds the boundary rather than
 * cutting the voice off mid-sentence. The limit only exists so a voice that never finishes — a stalled
 * stream, a hung request — cannot stall the whole tour with it.
 */
export const NARRATION_HOLD_LIMIT_MS = 15000;

/** What the voice is up to, as the clock needs to know it to pace itself. */
export interface TourClockNarration {
    /** The step whose line has been handed to the voice and not yet finished, or null when none has. */
    pendingStepIndex: number | null;

    /** The step whose line finished being said and when it did, or null when none just has. */
    finished: { stepIndex: number; finishedAt: number } | null;

    /** When the clock started holding a boundary for the voice, or null when it is not holding one. */
    holdStartedAt: number | null;
}

/** Where one tick of the tour's clock lands, and what it did to get there. */
export interface TourClockAdvance {
    /** Where the clock is after the tick, in milliseconds. */
    timeMs: number;

    /** Whether playback continues — false once the clock has reached the end of the tour. */
    isPlaying: boolean;

    /** When the in-progress boundary hold started, carried to the next tick, or null when not holding. */
    holdStartedAt: number | null;

    /** Whether the finished-narration marker was consumed by this tick and should be cleared. */
    finishedConsumed: boolean;
}

/**
 * Advances the tour's clock by one tick, pacing it against the voice.
 *
 * The clock leads and the voice follows — with two adjustments at the boundary between them. When a line
 * finishes ahead of its step's estimated length, the leftover is dead air: after a short beat the clock
 * skips to the step's end instead of sitting the estimate out. When a line is still being said as its
 * step's estimated length runs out, the clock holds just short of the boundary until the line finishes —
 * crossing it would tear the voice down mid-sentence — released by the same beat, and bounded by
 * {@link NARRATION_HOLD_LIMIT_MS} so a voice that never finishes cannot stall the tour.
 * @param timeline The tour laid out on one clock.
 * @param currentMs Where the clock is before the tick, in milliseconds.
 * @param elapsedMs How much wall-clock time the tick spans, in milliseconds.
 * @param now The wall clock at the moment of the tick, in milliseconds.
 * @param narration What the voice is up to.
 * @returns The {@link TourClockAdvance} the tick lands on.
 */
export function advanceTourClock(
    timeline: TourTimeline,
    currentMs: number,
    elapsedMs: number,
    now: number,
    narration: TourClockNarration): TourClockAdvance {
    const next = currentMs + elapsedMs;

    // Hold the boundary while the current step's line is still being said, or has finished but not yet had
    // its breath — the clock parks just short of the step's end so the step stays current and the voice
    // keeps its stage.
    const currentIndex = stepIndexAtTime(timeline, currentMs);
    const currentStep = currentIndex >= 0 ? timeline.steps[currentIndex] : undefined;
    if (currentStep && next >= currentStep.endMs) {
        const stillSpeaking = narration.pendingStepIndex === currentIndex;
        const settling = narration.finished !== null
            && narration.finished.stepIndex === currentIndex
            && now - narration.finished.finishedAt < NARRATION_ADVANCE_BEAT_MS;
        if (stillSpeaking || settling) {
            const holdStartedAt = narration.holdStartedAt ?? now;
            if (now - holdStartedAt < NARRATION_HOLD_LIMIT_MS) {
                return {
                    timeMs: Math.max(currentMs, currentStep.endMs - 1),
                    isPlaying: true,
                    holdStartedAt,
                    finishedConsumed: false,
                };
            }
        }
    }

    // The line has already been said — the rest of the step's estimated length is dead air. After the
    // breath, skip straight to the boundary instead of sitting the estimate out. The minimum play time
    // keeps the pointer's travel and press from being cut short by a line spoken faster than the
    // choreography.
    let finishedConsumed = false;
    const index = stepIndexAtTime(timeline, next);
    const timedStep = index >= 0 ? timeline.steps[index] : undefined;
    const finished = narration.finished;
    if (finished !== null) {
        if (!timedStep || finished.stepIndex !== index) {
            finishedConsumed = true;
        } else {
            const playedEnough = next >= timedStep.startMs + MINIMUM_STEP_MS;
            const breathed = now - finished.finishedAt >= NARRATION_ADVANCE_BEAT_MS;
            if (playedEnough && breathed && next < timedStep.endMs) {
                if (timedStep.endMs >= timeline.durationMs) {
                    return { timeMs: timeline.durationMs, isPlaying: false, holdStartedAt: null, finishedConsumed: true };
                }

                return { timeMs: timedStep.endMs, isPlaying: true, holdStartedAt: null, finishedConsumed: true };
            }
        }
    }

    if (next < timeline.durationMs) {
        return { timeMs: next, isPlaying: true, holdStartedAt: null, finishedConsumed };
    }

    return { timeMs: timeline.durationMs, isPlaying: false, holdStartedAt: null, finishedConsumed };
}
