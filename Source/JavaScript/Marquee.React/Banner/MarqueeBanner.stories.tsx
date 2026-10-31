// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { Meta, StoryObj } from '@storybook/react-vite';
import { bannersOfEveryImportance } from '../given/storyAnnouncements.js';
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
