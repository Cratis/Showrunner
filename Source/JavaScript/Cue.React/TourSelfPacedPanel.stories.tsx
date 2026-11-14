// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { Meta, StoryObj } from '@storybook/react';
// A small made-up set of chapters, as showcase data — the component itself depends only on the engine's own types.
import { sampleContent } from './given/sampleTour.js';
import { TourSelfPacedPanel } from './TourSelfPacedPanel.js';

const meta = {
    title: 'Cue/TourSelfPacedPanel',
    component: TourSelfPacedPanel,
    parameters: {
        layout: 'fullscreen',
        docs: {
            description: {
                component: 'The self-paced reading panel: the current chapter\'s written walkthrough, docked beside the chapter list when the tour is switched out of narrated playback.',
            },
        },
    },
    tags: ['autodocs'],
} satisfies Meta<typeof TourSelfPacedPanel>;

export default meta;
type Story = StoryObj<typeof meta>;

/** A chapter that opens with a list and a heading. */
export const Playground: Story = {
    args: {
        title: 'The desk',
        markdown: sampleContent['the-desk'],
    },
};

/** A chapter with a numbered list. */
export const WithSteps: Story = {
    args: {
        title: 'Picking',
        markdown: sampleContent['picking'],
    },
};

/** A chapter with a link and bold text. */
export const WithALink: Story = {
    args: {
        title: 'The cards',
        markdown: sampleContent['the-cards'],
    },
};

/** A chapter with a code block. */
export const WithCode: Story = {
    args: {
        title: 'Shipping',
        markdown: sampleContent['shipping'],
    },
};
