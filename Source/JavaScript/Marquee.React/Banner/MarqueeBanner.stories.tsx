// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { Meta, StoryObj } from '@storybook/react-vite';
import { userEvent, within } from 'storybook/test';
import { bannersOfEveryImportance, bannerWithMarkdownTitle, bannerWithMessage, bannerWithoutMessage, permanentBanner } from '../given/storyAnnouncements.js';
import { defaultMarqueeLabels } from '../Labels/index.js';
import { MarqueeBanner } from './MarqueeBanner.js';

const meta: Meta<typeof MarqueeBanner> = {
    title: 'Marquee/MarqueeBanner',
    component: MarqueeBanner,
    args: { labels: defaultMarqueeLabels }
};

export default meta;

type Story = StoryObj<typeof MarqueeBanner>;

export const Information: Story = { args: { announcements: [bannersOfEveryImportance[0]] } };
export const Warning: Story = { args: { announcements: [bannersOfEveryImportance[1]] } };
export const Critical: Story = { args: { announcements: [bannersOfEveryImportance[2]] } };
export const Several: Story = { args: { announcements: [...bannersOfEveryImportance].reverse() } };
export const None: Story = { args: { announcements: [] } };

/** A banner without a message is only its title: no button and no tooltip. */
export const WithoutMessage: Story = { args: { announcements: [bannerWithoutMessage] } };

/** A banner with a message shows a button at the end of the row. */
export const WithMessage: Story = { args: { announcements: [bannerWithMessage] } };

/** The button is clicked, and the message is shown inline below the title. */
export const Expanded: Story = {
    args: { announcements: [bannerWithMessage] },
    play: async ({ canvasElement }) => {
        await userEvent.click(within(canvasElement).getByRole('button', { name: defaultMarqueeLabels.expand }));
    }
};

/** The button is hovered, and the message is shown in a tooltip. */
export const Tooltip: Story = {
    args: { announcements: [bannerWithMessage] },
    play: async ({ canvasElement }) => {
        await userEvent.hover(within(canvasElement).getByRole('button', { name: defaultMarqueeLabels.expand }));
    }
};

/** A title with inline Markdown, here a link. */
export const MarkdownTitle: Story = { args: { announcements: [bannerWithMarkdownTitle] } };

/** An announcement with no start and no end is permanent: it stays until it is withdrawn. */
export const Permanent: Story = { args: { announcements: [permanentBanner] } };
