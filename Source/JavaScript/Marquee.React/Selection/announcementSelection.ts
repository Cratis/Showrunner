// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AnnouncementForMe, Display } from '@cratis/showrunner.marquee';

/**
 * What of the announcements a person is to see is on screen right now.
 */
export interface AnnouncementSelection {
    /** The banners to show, the most important first. */
    banners: AnnouncementForMe[];

    /** The dialogs to show one at a time, the most important first. Only the first is open. */
    dialogs: AnnouncementForMe[];
}

/**
 * Whether an announcement is inside its window at a moment. The window starts at `activeFrom` and ends just before
 * `activeUntil`, so an announcement that ends at 18:00 is gone at 18:00.
 * @param announcement The announcement.
 * @param now The moment.
 * @returns True when it is to be shown.
 */
export const isActiveAt = (announcement: AnnouncementForMe, now: Date): boolean =>
    announcement.activeFrom.getTime() <= now.getTime() && now.getTime() < announcement.activeUntil.getTime();

const byImportanceThenWindow = (first: AnnouncementForMe, second: AnnouncementForMe): number =>
    (second.importance - first.importance)
    || (first.activeFrom.getTime() - second.activeFrom.getTime())
    || first.id.toString().localeCompare(second.id.toString());

/**
 * Decides what is on screen from the announcements the backend returned, the clock and what the person closed.
 *
 * A banner is shown for as long as its window lasts: it can not be closed. A dialog is shown until it is closed, the
 * most important first, and a dialog closed in this session is not offered again in it.
 * @param announcements The announcements the backend returned, with their windows, which the backend does not apply.
 * @param now The moment to apply the windows at.
 * @param closedIds The ids of the dialogs closed in this session.
 * @returns What to show.
 */
export const selectAnnouncements = (announcements: AnnouncementForMe[], now: Date, closedIds: ReadonlySet<string>): AnnouncementSelection => {
    const active = announcements.filter(announcement => isActiveAt(announcement, now));

    return {
        banners: active.filter(announcement => announcement.display === Display.banner).sort(byImportanceThenWindow),
        dialogs: active
            .filter(announcement => announcement.display !== Display.banner && !closedIds.has(announcement.id.toString()))
            .sort(byImportanceThenWindow)
    };
};
