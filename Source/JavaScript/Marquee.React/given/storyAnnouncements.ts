// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Display, Importance } from '@cratis/showrunner.marquee';
import { anAnnouncement } from './an_announcement.js';

/** One announcement of each importance, as a banner, for the stories. */
export const bannersOfEveryImportance = [
    anAnnouncement({ title: 'Planned maintenance', body: 'We upgrade the database tonight from 22:00. See the [status page](https://status.example.com).', importance: Importance.information, display: Display.banner }),
    anAnnouncement({ title: 'Slow search', body: 'Search is slower than usual while we reindex.', importance: Importance.warning, display: Display.banner }),
    anAnnouncement({ title: 'Outage', body: 'Invoices can not be sent. We are working on it.', importance: Importance.critical, display: Display.banner })
];

/** A banner that is only a title: no message, so no button and no tooltip. */
export const bannerWithoutMessage = anAnnouncement({ title: 'Maintenance tonight', body: '', importance: Importance.warning, display: Display.banner });

/** A banner with a message behind its button. */
export const bannerWithMessage = anAnnouncement({
    title: 'Maintenance tonight',
    body: 'The ledger is **read-only** from 22:00 to 23:00 UTC. See the [status page](https://status.example.com).',
    importance: Importance.warning,
    display: Display.banner
});

/** A banner whose title has a link, and no message. */
export const bannerWithMarkdownTitle = anAnnouncement({
    title: 'Maintenance tonight. [Read more here.](https://status.example.com)',
    body: '',
    importance: Importance.information,
    display: Display.banner
});

/** A permanent banner: it starts immediately and never ends, as the backend sends it. */
export const permanentBanner = anAnnouncement({
    title: 'Ledger has a new address. [Read more here.](https://status.example.com/new-address)',
    body: 'Update your bookmarks.',
    importance: Importance.information,
    display: Display.banner,
    activeFrom: new Date('0001-01-01T00:00:00Z'),
    activeUntil: new Date('9999-12-31T23:59:59Z')
});

/** A dialog of the importance and display. */
export const aDialog = (importance: Importance, display: Display) => anAnnouncement({
    title: 'What is new in Reports',
    body: 'Reports now **export to Excel**.\n\n- Pick the columns\n- Schedule it\n\nRead [the guide](https://docs.example.com/reports) or write to [support](mailto:support@example.com).',
    importance,
    display
});
