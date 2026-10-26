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
 * Checks whether a support session is still running.
 * @param session The session.
 * @returns True while it runs.
 */
export function isSessionRunning(session: { ended: boolean }): boolean {
    return !session.ended;
}
