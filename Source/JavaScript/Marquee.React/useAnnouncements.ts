// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AcknowledgeAnnouncement, AnnouncementForMe, AnnouncementsForMe, Display } from '@cratis/showrunner.marquee';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { selectAnnouncements } from './Selection/index.js';

/**
 * How often the clock is read again to apply the windows of the announcements, in milliseconds.
 */
export const defaultCheckInterval = 60_000;

/**
 * Options for {@link useAnnouncements}.
 */
export interface UseAnnouncementsOptions {
    /** How often to read the clock again, in milliseconds. Defaults to 60 seconds. */
    checkInterval?: number;
}

/**
 * What {@link useAnnouncements} gives a component.
 */
export interface UsedAnnouncements {
    /** The banners to show, the most important first. */
    banners: AnnouncementForMe[];

    /** The dialogs still to show, the most important first. Only the first is to be open. */
    dialogs: AnnouncementForMe[];

    /**
     * Closes a dialog. A dialog shown once is acknowledged through the command, so it is gone on every device; a dialog
     * shown every session is closed for this session only, and returns in the next. A banner can not be closed.
     * @param announcement The dialog.
     * @returns Resolves when the acknowledgement was sent, when there was one.
     */
    close(announcement: AnnouncementForMe): Promise<void>;
}

/**
 * Observes the announcements of the signed-in person and applies their windows against the clock, so a product can draw
 * its own surface instead of {@link Marquee}'s. Must be used inside Arc's context, like every generated query.
 * @param options What to change.
 * @returns What to show, and how to close a dialog.
 */
export const useAnnouncements = ({ checkInterval = defaultCheckInterval }: UseAnnouncementsOptions = {}): UsedAnnouncements => {
    const [result] = AnnouncementsForMe.use();
    const [acknowledge] = AcknowledgeAnnouncement.use();
    const [now, setNow] = useState(() => new Date());
    const [closedIds, setClosedIds] = useState<ReadonlySet<string>>(() => new Set());

    useEffect(() => {
        const timer = setInterval(() => setNow(new Date()), checkInterval);
        return () => clearInterval(timer);
    }, [checkInterval]);

    const data = result.data;
    const { banners, dialogs } = useMemo(() => selectAnnouncements(data ?? [], now, closedIds), [data, now, closedIds]);

    const close = useCallback(async (announcement: AnnouncementForMe) => {
        setClosedIds(previous => new Set(previous).add(announcement.id.toString()));
        if (announcement.display === Display.dialogOnce) {
            acknowledge.announcementId = announcement.id;
            await acknowledge.execute();
        }
    }, [acknowledge]);

    return { banners, dialogs, close };
};
