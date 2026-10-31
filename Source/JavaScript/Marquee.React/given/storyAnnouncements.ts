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

/** A dialog of the importance and display. */
export const aDialog = (importance: Importance, display: Display) => anAnnouncement({
    title: 'What is new in Reports',
    body: 'Reports now **export to Excel**.\n\n- Pick the columns\n- Schedule it\n\nRead [the guide](https://docs.example.com/reports) or write to [support](mailto:support@example.com).',
    importance,
    display
});
