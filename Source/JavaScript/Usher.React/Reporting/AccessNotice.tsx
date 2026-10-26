// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Message } from '@cratis/components/Display';
import { AccessDuration } from '@cratis/showrunner.usher';
import { format } from '../Labels/index.js';
import { useUsherContext } from '../Provider/UsherContext.js';
import { formatTime, validUntilFor } from './AccessChoice.js';

/**
 * Props for {@link AccessNotice}.
 */
export interface AccessNoticeProps {
    /** The id of the notice, for the controls it describes. */
    id: string;

    /** For how long access would be granted. */
    duration: AccessDuration;
}

/**
 * States plainly what granting access means: support sees who the reporter is and can act as them until a given time.
 */
export const AccessNotice = ({ id, duration }: AccessNoticeProps) => {
    const { labels, locale } = useUsherContext();
    const time = formatTime(validUntilFor(duration, new Date()), locale);

    return (
        <div id={id} className='usher-access-notice'>
            <Message severity='info' live={false} text={format(labels.access.notice, { time })} />
        </div>
    );
};
