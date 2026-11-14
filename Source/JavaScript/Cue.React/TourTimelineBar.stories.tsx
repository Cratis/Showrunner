// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { Meta, StoryObj } from '@storybook/react';
import { fn } from 'storybook/test';
// A small made-up script, as showcase data — the component itself depends only on the engine's own types.
import { sampleTour } from './given/sampleTour.js';
import { TourTimelineBar } from './TourTimelineBar.js';
import type { TourPlayback } from './useTourPlayback.js';
import { buildTourTimeline } from './tourTimeline.js';

const timeline = buildTourTimeline(sampleTour);

const playbackWith = (overrides: Partial<TourPlayback>): TourPlayback => ({
    timeline,
    timeMs: timeline.durationMs / 3,
    isPlaying: true,
    isAudioEnabled: true,
    areSubtitlesEnabled: true,
    isSpeechAvailable: true,
    hasNarrationFailed: false,
    step: timeline.steps[0]?.step,
    chapterIndex: 1,
    sectionIndex: 0,
    togglePlay: fn(),
    pause: fn(),
    seekTo: fn(),
    goToChapter: fn(),
    goToPreviousChapter: fn(),
    goToNextChapter: fn(),
    toggleAudio: fn(),
    toggleSubtitles: fn(),
    ...overrides,
});

const meta = {
    title: 'Cue/TourTimelineBar',
    component: TourTimelineBar,
    parameters: {
        layout: 'fullscreen',
        docs: {
            description: {
                component: 'The guided tour\'s transport: play and pause, progress, chapter skipping, and the voice-over and subtitle switches.',
            },
        },
    },
    tags: ['autodocs'],
} satisfies Meta<typeof TourTimelineBar>;

export default meta;
type Story = StoryObj<typeof meta>;

/** Playing, with the voice-over and subtitles both on. */
export const Playground: Story = {
    args: { playback: playbackWith({}) },
};

/** Paused partway through. */
export const Paused: Story = {
    args: { playback: playbackWith({ isPlaying: false }) },
};

/** Both the voice-over and the subtitles switched off. */
export const Silenced: Story = {
    args: { playback: playbackWith({ isAudioEnabled: false, areSubtitlesEnabled: false }) },
};

/** No speech service configured, so the voice-over switch has nothing to turn on. */
export const WithoutSpeech: Story = {
    args: { playback: playbackWith({ isSpeechAvailable: false }) },
};

/** The voice-over failed mid-tour: the audio switch carries a quiet warning and subtitles carry on. */
export const NarrationFailed: Story = {
    args: { playback: playbackWith({ isAudioEnabled: false, hasNarrationFailed: true }) },
};

/** A tour with written chapters offers the self-paced switch beside the subtitle button. */
export const WithSelfPacedSwitch: Story = {
    args: { playback: playbackWith({}), onToggleSelfPaced: fn() },
};

/** Reading self-paced: the media controls dim while chapter skipping and the reading switch stay live. */
export const SelfPaced: Story = {
    args: { playback: playbackWith({ isPlaying: false }), isSelfPaced: true, onToggleSelfPaced: fn() },
};
