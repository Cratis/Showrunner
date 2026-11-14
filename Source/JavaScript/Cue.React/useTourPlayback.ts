// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useCueConfiguration } from './Configuration/CueProvider.js';
import type { TourSection } from './TourSection.js';
import type { TourStep } from './TourStep.js';
import { advanceTourClock } from './tourClock.js';
import {
    type TourTimeline,
    buildTourTimeline,
    chapterIndexAtTime,
    chapterStartMs,
    narrationsAfter,
    nextChapterTargetMs,
    previousChapterTargetMs,
    sectionIndexAtTime,
    stepIndexAtTime,
} from './tourTimeline.js';

// How many steps ahead of the one being said the voice-over is requested, so each line starts the moment its step does.
const NARRATION_LOOKAHEAD_STEPS = 2;

/** How often the clock advances. Fine enough for a progress bar, coarse enough not to re-render at frame rate. */
const TICK_MS = 100;

/** Everything the tour's chrome needs to draw itself and drive playback. */
export interface TourPlayback<TStep extends TourStep = TourStep> {
    /** The whole tour laid out on one clock. */
    timeline: TourTimeline<TStep>;

    /** Where playback has got to, in milliseconds. */
    timeMs: number;

    /** Whether the tour is running. */
    isPlaying: boolean;

    /** Whether the voice-over is switched on. */
    isAudioEnabled: boolean;

    /** Whether subtitles are switched on. */
    areSubtitlesEnabled: boolean;

    /** Whether a voice-over service is available at all. */
    isSpeechAvailable: boolean;

    /** Whether the voice-over has failed and the tour is carrying on with subtitles instead. */
    hasNarrationFailed: boolean;

    /** The step being played, or undefined when the script is empty. */
    step: TStep | undefined;

    /** The chapter being played. */
    chapterIndex: number;

    /** The section the playing chapter belongs to. */
    sectionIndex: number;

    /** Starts or stops playback. */
    togglePlay(): void;

    /** Stops playback where it is, whatever state it was in — what switching to self-paced reading uses. */
    pause(): void;

    /** Moves playback to a point on the clock. */
    seekTo(timeMs: number): void;

    /** Moves playback to the start of a chapter. */
    goToChapter(chapterIndex: number): void;

    /** Restarts the current chapter, or goes back one when barely into it. */
    goToPreviousChapter(): void;

    /** Skips to the next chapter. */
    goToNextChapter(): void;

    /** Switches the voice-over on or off. */
    toggleAudio(): void;

    /** Switches subtitles on or off. */
    toggleSubtitles(): void;
}

/**
 * Runs the guided tour: advances one clock, and keeps the voice-over pointed at whatever the clock says is
 * being narrated.
 * @param sections The scripted sections to play.
 * @returns The {@link TourPlayback} controls and state.
 * @remarks
 * The clock is the single source of truth — subtitles, progress, chapter highlighting and how much of the
 * tour's stage is showing are all read from it, so seeking anywhere is consistent by construction. Pacing
 * follows the schedule, with two adjustments at the boundaries where the schedule and the voice disagree
 * (see {@link advanceTourClock}): a line finishing ahead of its step's estimate skips the leftover dead air,
 * and a line still being said as the estimate runs out holds the clock at the step's end until it finishes —
 * bounded, so a voice that never finishes cannot stall the whole tour. Whatever is playing is always
 * silenced the moment the step it belongs to is left, whichever direction that happens in.
 */
export function useTourPlayback<TStep extends TourStep>(sections: TourSection<TStep>[]): TourPlayback<TStep> {
    const timeline = useMemo(() => buildTourTimeline(sections), [sections]);
    const [timeMs, setTimeState] = useState(0);
    // The clock's position, readable by the ticker without going through a state updater: an updater must not have
    // side effects, and the tick has one - it ends playback when the tour reaches its end.
    const timeMsRef = useRef(0);
    const setTimeMs = useCallback((next: number | ((current: number) => number)) => {
        timeMsRef.current = typeof next === 'function' ? next(timeMsRef.current) : next;
        setTimeState(timeMsRef.current);
    }, []);
    const [isPlaying, setIsPlaying] = useState(true);
    const [isAudioEnabled, setIsAudioEnabled] = useState(true);
    const [areSubtitlesEnabled, setAreSubtitlesEnabled] = useState(true);
    const [hasNarrationFailed, setHasNarrationFailed] = useState(false);
    const { useSpeech } = useCueConfiguration();
    const { isAvailable: isSpeechAvailable, isPreparing, isSpeaking, hasError: hasSpeechError, speak, prefetch, stop } = useSpeech();

    const stepIndex = stepIndexAtTime(timeline, timeMs);
    const step = stepIndex >= 0 ? timeline.steps[stepIndex].step : undefined;
    const chapterIndex = chapterIndexAtTime(timeline, timeMs);
    const sectionIndex = sectionIndexAtTime(timeline, timeMs);

    // Which step the voice has already been given. Reset to nothing whenever playback jumps or is silenced,
    // so resuming or seeking back always speaks the line again rather than sitting through it in silence.
    const narratedStepRef = useRef(-1);

    // The narration currently being watched for completion: set when a line is handed to the voice, marked
    // started once audio is actually being prepared or spoken, and cleared once it is known to have ended.
    // Refs rather than state — nothing here needs to trigger its own render.
    const pendingNarrationRef = useRef<{ stepIndex: number; started: boolean } | null>(null);
    const narrationFinishedRef = useRef<{ stepIndex: number; finishedAt: number } | null>(null);
    const narrationHoldRef = useRef<number | null>(null);

    useEffect(() => {
        if (!isPlaying) return;

        let previous = performance.now();
        const ticker = setInterval(() => {
            const now = performance.now();
            const elapsed = now - previous;
            previous = now;
            const advance = advanceTourClock(timeline, timeMsRef.current, elapsed, now, {
                pendingStepIndex: pendingNarrationRef.current?.stepIndex ?? null,
                finished: narrationFinishedRef.current,
                holdStartedAt: narrationHoldRef.current,
            });
            narrationHoldRef.current = advance.holdStartedAt;
            if (advance.finishedConsumed) {
                narrationFinishedRef.current = null;
            }

            setTimeMs(advance.timeMs);
            if (!advance.isPlaying) {
                setIsPlaying(false);
            }
        }, TICK_MS);

        return () => clearInterval(ticker);
    }, [isPlaying, timeline, setTimeMs]);

    useEffect(() => {
        if (!isPlaying || !isAudioEnabled || !isSpeechAvailable) {
            narratedStepRef.current = -1;
            pendingNarrationRef.current = null;
            narrationFinishedRef.current = null;
            narrationHoldRef.current = null;
            stop();
            return;
        }

        if (narratedStepRef.current === stepIndex) return;
        narratedStepRef.current = stepIndex;

        narrationFinishedRef.current = null;
        const narration = stepIndex >= 0 ? timeline.steps[stepIndex].step.narration : '';
        if (narration.length > 0) {
            pendingNarrationRef.current = { stepIndex, started: false };

            // speak() tears down whatever the voice was still saying for the step just left, before
            // starting this step's line — leaving a step always silences it, never lets it bleed forward.
            void speak(narration);
        } else {
            // A silent, camera-only step — nothing to say, but anything still playing from the step before
            // it must be cut off rather than left to talk over a scene it no longer belongs to.
            pendingNarrationRef.current = null;
            stop();
        }
    }, [isPlaying, isAudioEnabled, isSpeechAvailable, stepIndex, timeline, speak, stop]);

    // Asks the voice for the lines still to come while the current one is being said, so each starts the moment its
    // step does instead of waiting for the speech service. The current line goes first: nothing is requested until it
    // is audible, so the lookahead never competes with the line the viewer is waiting for.
    useEffect(() => {
        if (!isPlaying || !isAudioEnabled || !isSpeechAvailable || stepIndex < 0) return;

        // The line being waited for has been handed to the voice but is not audible yet: ask for nothing else.
        const pending = pendingNarrationRef.current;
        if (pending && (!pending.started || isPreparing)) return;

        for (const narration of narrationsAfter(timeline, stepIndex, NARRATION_LOOKAHEAD_STEPS)) prefetch(narration);
    }, [isPlaying, isAudioEnabled, isSpeechAvailable, isPreparing, stepIndex, timeline, prefetch]);

    // Watches the voice to notice when the pending line has finished. "Finished" is a transition — the line
    // must have been heard preparing or speaking before silence counts as its end, otherwise the quiet moment
    // between asking for a clip and the first audio arriving would be mistaken for completion.
    useEffect(() => {
        const pending = pendingNarrationRef.current;
        if (!pending) return;

        if (isPreparing || isSpeaking) {
            pending.started = true;
            return;
        }

        if (pending.started) {
            pendingNarrationRef.current = null;
            narrationFinishedRef.current = { stepIndex: pending.stepIndex, finishedAt: performance.now() };
        }
    }, [isPreparing, isSpeaking]);

    // A failing voice-over must not make the tour silently silent: switch the audio toggle off so the
    // transport shows what happened, and make sure subtitles are on so the words still reach the viewer.
    // The clock keeps running throughout — narration failing never stalls the tour.
    useEffect(() => {
        if (!hasSpeechError || !isAudioEnabled) return;

        setHasNarrationFailed(true);
        setIsAudioEnabled(false);
        setAreSubtitlesEnabled(true);
    }, [hasSpeechError, isAudioEnabled]);

    const seekTo = useCallback((target: number) => {
        narratedStepRef.current = -1;
        pendingNarrationRef.current = null;
        narrationFinishedRef.current = null;
        narrationHoldRef.current = null;
        setTimeMs(Math.min(Math.max(0, target), timeline.durationMs));
    }, [timeline.durationMs, setTimeMs]);

    const isPlayingRef = useRef(isPlaying);
    isPlayingRef.current = isPlaying;
    const togglePlay = useCallback(() => {
        // Pressing play at the very end starts over rather than doing nothing, which is what a finished
        // video does and what someone reaching for the button after it ends is asking for.
        if (!isPlayingRef.current) {
            if (timeMsRef.current >= timeline.durationMs) setTimeMs(0);
            narratedStepRef.current = -1;
        }

        setIsPlaying(!isPlayingRef.current);
    }, [timeline.durationMs, setTimeMs]);

    const pause = useCallback(() => setIsPlaying(false), []);

    const goToChapter = useCallback((target: number) => seekTo(chapterStartMs(timeline, target)), [seekTo, timeline]);
    const goToPreviousChapter = useCallback(() => seekTo(previousChapterTargetMs(timeline, timeMs)), [seekTo, timeline, timeMs]);
    const goToNextChapter = useCallback(() => seekTo(nextChapterTargetMs(timeline, timeMs)), [seekTo, timeline, timeMs]);

    // Turning the voice-over back on is also how a failed narration is retried, so the failure flag clears
    // and the current line is spoken afresh.
    const isAudioEnabledRef = useRef(isAudioEnabled);
    isAudioEnabledRef.current = isAudioEnabled;
    const toggleAudio = useCallback(() => {
        if (!isAudioEnabledRef.current) {
            setHasNarrationFailed(false);
            narratedStepRef.current = -1;
        }

        setIsAudioEnabled(!isAudioEnabledRef.current);
    }, []);

    const toggleSubtitles = useCallback(() => setAreSubtitlesEnabled(current => !current), []);

    return {
        timeline,
        timeMs,
        isPlaying,
        isAudioEnabled,
        areSubtitlesEnabled,
        isSpeechAvailable,
        hasNarrationFailed,
        step,
        chapterIndex,
        sectionIndex,
        togglePlay,
        pause,
        seekTo,
        goToChapter,
        goToPreviousChapter,
        goToNextChapter,
        toggleAudio,
        toggleSubtitles,
    };
}
