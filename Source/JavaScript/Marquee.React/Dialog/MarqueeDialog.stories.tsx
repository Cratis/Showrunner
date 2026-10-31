// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Display, Importance } from '@cratis/showrunner.marquee';
import type { Meta, StoryObj } from '@storybook/react-vite';
import { aDialog } from '../given/storyAnnouncements.js';
import { defaultMarqueeLabels } from '../Labels/index.js';
import { MarqueeDialog } from './MarqueeDialog.js';

const meta: Meta<typeof MarqueeDialog> = {
    title: 'Marquee/MarqueeDialog',
    component: MarqueeDialog,
    args: { hasMore: false, labels: defaultMarqueeLabels, onClose: () => { /* a story closes nothing */ } }
};

export default meta;

type Story = StoryObj<typeof MarqueeDialog>;

export const DialogOnceInformation: Story = { args: { announcement: aDialog(Importance.information, Display.dialogOnce) } };
export const DialogOnceWarning: Story = { args: { announcement: aDialog(Importance.warning, Display.dialogOnce) } };
export const DialogOnceCritical: Story = { args: { announcement: aDialog(Importance.critical, Display.dialogOnce) } };
export const DialogEverySessionInformation: Story = { args: { announcement: aDialog(Importance.information, Display.dialogEverySession) } };
export const DialogEverySessionWarning: Story = { args: { announcement: aDialog(Importance.warning, Display.dialogEverySession) } };
export const DialogEverySessionCritical: Story = { args: { announcement: aDialog(Importance.critical, Display.dialogEverySession) } };
export const WithMoreToFollow: Story = { args: { announcement: aDialog(Importance.warning, Display.dialogOnce), hasMore: true } };
