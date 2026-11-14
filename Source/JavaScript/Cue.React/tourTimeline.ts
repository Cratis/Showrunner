// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { TourChapter } from './TourChapter.js';
import type { TourSection } from './TourSection.js';
import type { TourStep } from './TourStep.js';

/** Words per minute the narration is assumed to be read at — a calm, unhurried voice-over pace. */
const NARRATION_WORDS_PER_MINUTE = 145;

/** The breath left at the end of a narrated step before the next one starts. */
const NARRATION_TAIL_MS = 900;

/** No step is ever shorter than this, however short its line is. */
export const MINIMUM_STEP_MS = 2200;

/** How long a step with nothing to say lasts — a pure camera move. */
const SILENT_STEP_MS = 2500;

/** How far into a chapter "previous" restarts it rather than going back a chapter, as a video player does. */
export const RESTART_CHAPTER_WINDOW_MS = 2000;

/** A step placed on the tour's clock. */
export interface TimedTourStep<TStep extends TourStep = TourStep> {
    /** The step itself. */
    step: TStep;

    /** The chapter the step belongs to. */
    chapterIndex: number;

    /** When the step starts, in milliseconds from the beginning of the tour. */
    startMs: number;

    /** When the step ends, in milliseconds from the beginning of the tour. */
    endMs: number;
}

/** A chapter placed on the tour's clock. */
export interface TimedTourChapter<TStep extends TourStep = TourStep> {
    /** The chapter itself. */
    chapter: TourChapter<TStep>;

    /** The section the chapter belongs to. */
    sectionIndex: number;

    /** When the chapter starts, in milliseconds from the beginning of the tour. */
    startMs: number;

    /** When the chapter ends, in milliseconds from the beginning of the tour. */
    endMs: number;
}

/** A section placed on the tour's clock. */
export interface TimedTourSection<TStep extends TourStep = TourStep> {
    /** The section itself. */
    section: TourSection<TStep>;

    /** When the section starts, in milliseconds from the beginning of the tour. */
    startMs: number;

    /** When the section ends, in milliseconds from the beginning of the tour. */
    endMs: number;
}

/** The whole tour laid out on one clock, which is what the progress bar, subtitles and chapter list read. */
export interface TourTimeline<TStep extends TourStep = TourStep> {
    /** Every step of every chapter of every section, in play order. */
    steps: TimedTourStep<TStep>[];

    /** Every chapter, in play order across all sections. Chapter indices used anywhere in the player index into this list. */
    chapters: TimedTourChapter<TStep>[];

    /** Every section, in play order. */
    sections: TimedTourSection<TStep>[];

    /** How long the whole tour lasts, in milliseconds. */
    durationMs: number;
}

/**
 * Estimates how long a line of narration takes to say.
 * @param narration The line to be spoken.
 * @returns The duration in milliseconds.
 */
export function narrationDurationMs(narration: string): number {
    const words = narration.split(/\s+/u).filter(word => word.length > 0).length;
    if (words === 0) return SILENT_STEP_MS;

    const spoken = (words / NARRATION_WORDS_PER_MINUTE) * 60000;
    return Math.max(MINIMUM_STEP_MS, Math.round(spoken + NARRATION_TAIL_MS));
}

/**
 * Gets how long a step lasts, preferring the script's own figure when it states one.
 * @param step The step to measure.
 * @returns The duration in milliseconds.
 */
export function stepDurationMs(step: TourStep): number {
    return step.durationMs ?? narrationDurationMs(step.narration);
}

/**
 * Lays the sections out on a single clock so every part of the player can work in one shared unit of time.
 *
 * The clock stays flat: steps and chapters run straight through every section, and a chapter's index is its
 * position across the whole tour — sections only add headings and spans on top, they never fragment the clock.
 * @param sections The scripted sections, in play order.
 * @returns The {@link TourTimeline}.
 */
export function buildTourTimeline<TStep extends TourStep>(sections: TourSection<TStep>[]): TourTimeline<TStep> {
    const steps: TimedTourStep<TStep>[] = [];
    const timedChapters: TimedTourChapter<TStep>[] = [];
    const timedSections: TimedTourSection<TStep>[] = [];
    let cursor = 0;

    sections.forEach((section, sectionIndex) => {
        const sectionStart = cursor;
        for (const chapter of section.chapters) {
            const chapterStart = cursor;
            const chapterIndex = timedChapters.length;
            for (const step of chapter.steps) {
                const endMs = cursor + stepDurationMs(step);
                steps.push({ step, chapterIndex, startMs: cursor, endMs });
                cursor = endMs;
            }

            timedChapters.push({ chapter, sectionIndex, startMs: chapterStart, endMs: cursor });
        }

        timedSections.push({ section, startMs: sectionStart, endMs: cursor });
    });

    return { steps, chapters: timedChapters, sections: timedSections, durationMs: cursor };
}

/**
 * Gets which step is playing at a point on the clock.
 * @param timeline The timeline to look in.
 * @param timeMs The point on the clock, in milliseconds.
 * @returns The index into {@link TourTimeline.steps}, or -1 when the timeline is empty.
 */
export function stepIndexAtTime(timeline: TourTimeline, timeMs: number): number {
    if (timeline.steps.length === 0) return -1;

    // The end of the tour belongs to its last step rather than to nothing, so the final line stays on screen
    // when playback finishes instead of the subtitles blanking on the last frame.
    const found = timeline.steps.findIndex(candidate => timeMs >= candidate.startMs && timeMs < candidate.endMs);
    if (found >= 0) return found;

    return timeMs < 0 ? 0 : timeline.steps.length - 1;
}

/**
 * Gets which chapter is playing at a point on the clock.
 * @param timeline The timeline to look in.
 * @param timeMs The point on the clock, in milliseconds.
 * @returns The index into {@link TourTimeline.chapters}, or -1 when the timeline is empty.
 */
export function chapterIndexAtTime(timeline: TourTimeline, timeMs: number): number {
    const stepIndex = stepIndexAtTime(timeline, timeMs);
    return stepIndex < 0 ? -1 : timeline.steps[stepIndex].chapterIndex;
}

/**
 * Gets which section is playing at a point on the clock.
 * @param timeline The timeline to look in.
 * @param timeMs The point on the clock, in milliseconds.
 * @returns The index into {@link TourTimeline.sections}, or -1 when the timeline is empty.
 */
export function sectionIndexAtTime(timeline: TourTimeline, timeMs: number): number {
    const chapterIndex = chapterIndexAtTime(timeline, timeMs);
    return chapterIndex < 0 ? -1 : timeline.chapters[chapterIndex].sectionIndex;
}

/**
 * Gets where on the clock a chapter begins.
 * @param timeline The timeline to look in.
 * @param chapterIndex The chapter to locate.
 * @returns The point on the clock in milliseconds, or 0 when there is no such chapter.
 */
export function chapterStartMs(timeline: TourTimeline, chapterIndex: number): number {
    return timeline.chapters[chapterIndex]?.startMs ?? 0;
}

/**
 * Gets where "previous chapter" should land from a point on the clock.
 * @param timeline The timeline to look in.
 * @param timeMs The current point on the clock, in milliseconds.
 * @returns The point on the clock to jump to, in milliseconds.
 */
export function previousChapterTargetMs(timeline: TourTimeline, timeMs: number): number {
    const current = chapterIndexAtTime(timeline, timeMs);
    if (current < 0) return 0;

    // Well into a chapter, "previous" restarts the one being watched — the same thing a video player does,
    // and what someone who missed the start of an explanation actually wants.
    const startOfCurrent = chapterStartMs(timeline, current);
    if (timeMs - startOfCurrent > RESTART_CHAPTER_WINDOW_MS) return startOfCurrent;

    return chapterStartMs(timeline, Math.max(0, current - 1));
}

/**
 * Gets where "next chapter" should land from a point on the clock.
 * @param timeline The timeline to look in.
 * @param timeMs The current point on the clock, in milliseconds.
 * @returns The point on the clock to jump to, in milliseconds.
 */
export function nextChapterTargetMs(timeline: TourTimeline, timeMs: number): number {
    const current = chapterIndexAtTime(timeline, timeMs);
    if (current < 0) return 0;
    if (current >= timeline.chapters.length - 1) return timeline.durationMs;

    return chapterStartMs(timeline, current + 1);
}

// The lines still to come after a step that have something to say, in the order they will be needed — what the voice-over is
// asked for ahead of time so each starts the moment its step does.
export function narrationsAfter(timeline: TourTimeline, stepIndex: number, count: number): string[] {
    if (stepIndex < 0) return [];
    return timeline.steps
        .slice(stepIndex + 1, stepIndex + 1 + count)
        .map(timed => timed.step.narration)
        .filter(narration => narration.length > 0);
}

/**
 * Formats a point on the clock the way a video player does.
 * @param timeMs The point on the clock, in milliseconds.
 * @returns The time as `m:ss`.
 */
export function formatTourTime(timeMs: number): string {
    const totalSeconds = Math.max(0, Math.floor(timeMs / 1000));
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
}
