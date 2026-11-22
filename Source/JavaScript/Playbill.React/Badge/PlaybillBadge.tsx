// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReactNode, useMemo } from 'react';
import { PlaybillIcon } from '../Icons.js';
import { PlaybillLabels, resolveLabels, withCount } from '../Labels/index.js';
import { usePlaybillStatus } from '../usePlaybillStatus.js';

/**
 * Props for {@link PlaybillBadge}.
 */
export interface PlaybillBadgeProps {
    /** Called when the badge is pressed; open the release notes page or dialog from it. */
    onClick?: () => void;
    /** The texts to replace; the rest stay English. */
    labels?: Partial<PlaybillLabels>;
    /** Leaves the badge out when nothing is unread. By default it stays, so the history is always one press away. */
    hideWhenRead?: boolean;
    /** Replaces the icon. */
    children?: ReactNode;
}

/**
 * A button for the application's header that tells the person how many releases they have not seen the notes of. It
 * updates on every device when the notes are seen on one of them. The count is a number on the button; its accessible
 * name says it in words.
 */
export const PlaybillBadge = ({ onClick, labels, hideWhenRead = false, children }: PlaybillBadgeProps) => {
    const { unread } = usePlaybillStatus();
    const resolved = useMemo(() => resolveLabels(labels), [labels]);
    if (unread === 0 && hideWhenRead) {
        return <></>;
    }

    return (
        <button
            type='button'
            className={unread > 0 ? 'playbill-badge playbill-badge--unread' : 'playbill-badge'}
            aria-label={unread > 0 ? withCount(resolved.badgeUnread, unread) : resolved.badge}
            onClick={onClick}>
            {children ?? <PlaybillIcon />}
            {unread > 0 && <span className='playbill-badge__count' aria-hidden='true'>{unread}</span>}
        </button>
    );
};
