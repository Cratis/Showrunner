// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { Meta, StoryObj } from '@storybook/react';
import { fn } from 'storybook/test';
// A small made-up script, as showcase data — the component itself depends only on the engine's own types.
import { sampleTour } from './given/sampleTour.js';
import { TourChapterList } from './TourChapterList.js';
import { buildTourTimeline } from './tourTimeline.js';

const timeline = buildTourTimeline(sampleTour);

const meta = {
    title: 'Cue/TourChapterList',
    component: TourChapterList,
    parameters: {
        layout: 'fullscreen',
        docs: {
            description: {
                component: 'The guided tour\'s table of contents — sections as headings with their chapters under them — showing where the viewer is and letting them jump anywhere.',
            },
        },
    },
    tags: ['autodocs'],
} satisfies Meta<typeof TourChapterList>;

export default meta;
type Story = StoryObj<typeof meta>;

/** The list as it looks partway through the basics. */
export const Playground: Story = {
    args: {
        sections: timeline.sections,
        chapters: timeline.chapters,
        currentChapterIndex: 2,
        onSelectChapter: fn(),
    },
};

/** The list at the very start, before anything has played. */
export const AtTheBeginning: Story = {
    args: {
        sections: timeline.sections,
        chapters: timeline.chapters,
        currentChapterIndex: 0,
        onSelectChapter: fn(),
    },
};

/** Playing a chapter in one of the later sections, to show the grouping and the highlight together. */
export const InALaterSection: Story = {
    args: {
        sections: timeline.sections,
        chapters: timeline.chapters,
        currentChapterIndex: timeline.chapters.length - 1,
        onSelectChapter: fn(),
    },
};

/** Reading self-paced: the timings disappear, because there is no clock to time against. */
export const WithoutTimings: Story = {
    args: {
        sections: timeline.sections,
        chapters: timeline.chapters,
        currentChapterIndex: 2,
        onSelectChapter: fn(),
        showTimings: false,
    },
};
