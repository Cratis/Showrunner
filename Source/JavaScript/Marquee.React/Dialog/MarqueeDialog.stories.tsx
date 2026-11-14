// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Display, Importance } from '@cratis/showrunner.marquee';
import type { Meta, StoryObj } from '@storybook/react-vite';
import { anAnnouncement } from '../given/an_announcement.js';
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

/** A title with a link is the heading as plain text, and is shown again with its link at the top of the content. */
export const MarkdownTitle: Story = { args: { announcement: anAnnouncement({ title: 'Maintenance tonight. [Read more here.](https://status.example.com)', display: Display.dialogOnce }) } };

/** A message is optional: the dialog is only its title. */
export const WithoutMessage: Story = { args: { announcement: anAnnouncement({ title: 'Maintenance tonight', body: '', display: Display.dialogOnce }) } };
