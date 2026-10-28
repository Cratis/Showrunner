// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ConversationEntry, SupportSession } from '@cratis/showrunner.usher';
import { format } from '../../Labels/index.js';
import { useUsherContext } from '../../Provider/UsherContext.js';
import { formatClock, formatTime } from '../../Reporting/AccessChoice.js';
import { isSessionRunning } from '../accessStatus.js';
import { sessionActivityOf } from '../sessionActivity.js';

/**
 * Props for {@link SupportSessions}.
 */
export interface SupportSessionsProps {
    /** The sessions, oldest first. */
    sessions: SupportSession[];

    /** The entries of the conversation, to show what support did in each session. */
    entries: ConversationEntry[];
}

/**
 * Lists the sessions in which support signed in as the reporter, each with what support did as the reporter in it.
 */
export const SupportSessions = ({ sessions, entries }: SupportSessionsProps) => {
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
                    const activity = sessionActivityOf(session, entries, labels.accessPanel.sessionActions);
                    return (
                        <li key={session.sessionId.toString()}>
                            {format(labels.accessPanel.session, { name, start, end })}
                            {activity.length > 0
                                ? (
                                    <ul className='usher-sessions__activity'>
                                        {activity.map(action => <li key={action.entryId}>{action.text}</li>)}
                                    </ul>
                                )
                                : <p className='usher-sessions__activity usher-panel__text--secondary'>{labels.accessPanel.sessionDidNothing}</p>}
                        </li>
                    );
                })}
            </ul>
        </div>
    );
};

const asDate = (value: Date | string) => value instanceof Date ? value : new Date(value);
