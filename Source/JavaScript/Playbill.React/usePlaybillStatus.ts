// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ForMe, MarkReleaseNotesSeen } from '@cratis/showrunner.playbill';
import { useCallback } from 'react';

/**
 * What {@link usePlaybillStatus} gives a component.
 */
export interface UsedPlaybillStatus {
    /** The newest release the person has seen the notes of; absent when they have seen none. */
    lastSeen?: string;
    /** How many releases they have not seen the notes of. */
    unread: number;
    /** The versions of those releases, the newest first. */
    unreadVersions: string[];
    /**
     * Records that the person has seen the release notes up to a version, on every device. It only moves forward, and
     * marking a release they have seen already does nothing.
     * @param version The newest release the person has seen the notes of.
     * @returns Resolves when the command has been sent.
     */
    markSeen(version: string): Promise<void>;
}

/**
 * Observes which releases the signed-in person has not been told about, and lets a component record that they have.
 * Must be used inside Arc's context, like every generated query. It needs the backend package
 * `Cratis.Showrunner.Playbill`.
 * @returns What the person has seen and how to record that they have seen more.
 */
export const usePlaybillStatus = (): UsedPlaybillStatus => {
    const [result] = ForMe.use();
    const [mark] = MarkReleaseNotesSeen.use();
    const status = result.data?.[0];

    const markSeen = useCallback(async (version: string) => {
        mark.version = version;
        await mark.execute();
    }, [mark]);

    return {
        lastSeen: status?.lastSeen || undefined,
        unread: status?.unread ?? 0,
        unreadVersions: [...status?.unreadVersions ?? []],
        markSeen
    };
};
