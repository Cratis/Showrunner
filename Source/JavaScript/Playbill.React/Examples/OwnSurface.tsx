// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { usePlaybill } from '@cratis/showrunner.playbill.react';

export const OwnSurface = () => {
    const { releases, unread, unreadVersions, markSeen } = usePlaybill();
    const newest = releases[0];

    return (
        <aside>
            <strong>{unread} new</strong>
            {newest && <p>{newest.version}: {newest.summary}</p>}
            <button type='button' onClick={() => void markSeen(unreadVersions[0])} disabled={unread === 0}>
                Mark as read
            </button>
        </aside>
    );
};
