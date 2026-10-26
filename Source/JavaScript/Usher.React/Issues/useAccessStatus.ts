// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { IssueAccess } from '@cratis/showrunner.usher';
import { useEffect, useState } from 'react';
import { AccessStatus, accessStatusOf, nextAccessStatusCheckIn } from './accessStatus.js';

/**
 * Decides whether support may sign in as the reporter, and decides again when a grant reaches its end, or at the
 * latest once a minute, so a grant that runs out while the view is open is shown as over
 * without waiting for the backend to record the lapse.
 * @param access The {@link IssueAccess}; nothing when the issue has none yet.
 * @returns The {@link AccessStatus} now, and the time it was decided at.
 */
export function useAccessStatus(access: IssueAccess | undefined): [AccessStatus, Date] {
    const [now, setNow] = useState(() => new Date());

    useEffect(() => {
        const timer = setTimeout(() => setNow(new Date()), nextAccessStatusCheckIn(access, new Date()));
        return () => clearTimeout(timer);
    }, [access, now]);

    return [accessStatusOf(access, now), now];
}
