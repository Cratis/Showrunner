// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SupportSession } from '@cratis/showrunner.usher';
import { format } from '../../Labels/index.js';
import { useUsherContext } from '../../Provider/UsherContext.js';
import { formatClock, formatTime } from '../../Reporting/AccessChoice.js';
import { isSessionRunning } from '../accessStatus.js';

/**
 * Props for {@link SupportSessions}.
 */
export interface SupportSessionsProps {
    /** The sessions, oldest first. */
    sessions: SupportSession[];
}

/**
 * Lists the sessions in which support signed in as the reporter.
 */
export const SupportSessions = ({ sessions }: SupportSessionsProps) => {
    const { labels, locale } = useUsherContext();

    if (sessions.length === 0) {
        return <p className='usher-panel__text usher-panel__text--secondary'>{labels.accessPanel.noSessions}</p>;
    }

    return (
        <div className='usher-sessions'>
            <span className='usher-sessions__heading'>{labels.accessPanel.sessionsHeading}</span>
            <ul className='usher-sessions__list'>
                {sessions.map(session => {
                    const start = formatTime(asDate(session.startedAt), locale);
                    const end = isSessionRunning(session) ? labels.accessPanel.sessionRunning : formatClock(asDate(session.endedAt), locale);
                    const name = (session.author?.name ?? '').length > 0 ? session.author.name : labels.conversation.support;
                    return <li key={session.sessionId.toString()}>{format(labels.accessPanel.session, { name, start, end })}</li>;
                })}
            </ul>
        </div>
    );
};

const asDate = (value: Date | string) => value instanceof Date ? value : new Date(value);
