// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { AnnouncementForMe, Display, Importance } from '@cratis/showrunner.marquee';

export interface AnnouncementValues {
    id?: Guid;
    title?: string;
    body?: string;
    display?: Display;
    importance?: Importance;
    activeFrom?: Date;
    activeUntil?: Date;
}

/** An announcement that is active from the start of 2026 to the end of 2099 unless said otherwise. */
export const anAnnouncement = (values: AnnouncementValues = {}): AnnouncementForMe => {
    const announcement = new AnnouncementForMe();
    announcement.id = values.id ?? Guid.create();
    announcement.title = values.title ?? 'Planned maintenance';
    announcement.body = values.body ?? 'We are **upgrading** the database tonight.';
    announcement.display = values.display ?? Display.banner;
    announcement.importance = values.importance ?? Importance.information;
    announcement.activeFrom = values.activeFrom ?? new Date('2026-01-01T00:00:00Z');
    announcement.activeUntil = values.activeUntil ?? new Date('2099-12-31T23:59:59Z');
    return announcement;
};
