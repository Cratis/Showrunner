// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AccessState, IssueAccess } from '@cratis/showrunner.usher';

/**
 * Whether support may sign in as the reporter on an issue, as the access panel shows it.
 */
export enum AccessStatus {
    /** Support may not sign in, and does not ask to. */
    notGranted = 'notGranted',

    /** Support asks to sign in. */
    requested = 'requested',

    /** Support may sign in until the grant ends. */
    granted = 'granted'
}

/**
 * Decides whether support may sign in as the reporter, against the clock: a grant that has passed its end is over,
 * even before the backend records that it lapsed.
 * @param access The {@link IssueAccess}; nothing when the issue has none yet.
 * @param now What time it is.
 * @returns The {@link AccessStatus}.
 */
export function accessStatusOf(access: IssueAccess | undefined, now: Date): AccessStatus {
    if (!access) {
        return AccessStatus.notGranted;
    }

    const validUntil = access.validUntil instanceof Date ? access.validUntil : new Date(access.validUntil);
    if (access.state === AccessState.granted && validUntil.getTime() > now.getTime()) {
        return AccessStatus.granted;
    }

    return access.request?.pending ? AccessStatus.requested : AccessStatus.notGranted;
}

/**
 * The longest the access panel waits before it decides the status again, so it stays right even when the clock jumps.
 */
export const AccessStatusRefreshInterval = 60_000;

/**
 * Gets how long until the status changes by itself: a live grant ends at its end. Nothing else changes the status
 * without a new fact from the backend.
 * @param access The {@link IssueAccess}; nothing when the issue has none yet.
 * @param now What time it is.
 * @returns How many milliseconds until the status is decided again, at most {@link AccessStatusRefreshInterval}.
 */
export function nextAccessStatusCheckIn(access: IssueAccess | undefined, now: Date): number {
    if (accessStatusOf(access, now) !== AccessStatus.granted) {
        return AccessStatusRefreshInterval;
    }

    const validUntil = access!.validUntil instanceof Date ? access!.validUntil : new Date(access!.validUntil);
    return Math.min(Math.max(validUntil.getTime() - now.getTime(), 0), AccessStatusRefreshInterval);
}

/**
 * Checks whether a support session is still running.
 * @param session The session.
 * @returns True while it runs.
 */
export function isSessionRunning(session: { ended: boolean }): boolean {
    return !session.ended;
}
