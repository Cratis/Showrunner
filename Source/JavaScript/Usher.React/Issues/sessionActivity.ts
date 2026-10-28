// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ConversationEntry, ConversationEntryKind, SupportSession } from '@cratis/showrunner.usher';
import { format, SessionActionLabels } from '../Labels/index.js';

const actionLabelKeys: Partial<Record<ConversationEntryKind, keyof SessionActionLabels>> = {
    [ConversationEntryKind.detailsAdded]: 'detailsAdded',
    [ConversationEntryKind.questionAnswered]: 'questionAnswered',
    [ConversationEntryKind.identityShared]: 'identityShared',
    [ConversationEntryKind.reopened]: 'reopened',
    [ConversationEntryKind.withdrawn]: 'withdrawn',
    [ConversationEntryKind.accessRevoked]: 'accessRevoked'
};

/**
 * What support did while signed in as the reporter in one session.
 */
export interface SessionActivity {
    /** The entry the action is. */
    readonly entryId: string;

    /** What was done, as the reporter is told. */
    readonly text: string;
}

/**
 * Gets what support did as the reporter in a session, oldest first: the entries the backend credits to the session,
 * without the session's own start and end.
 * @param session The {@link SupportSession}.
 * @param entries The entries of the conversation.
 * @param labels The {@link SessionActionLabels}.
 * @returns The {@link SessionActivity} of the session.
 */
export function sessionActivityOf(session: SupportSession, entries: ConversationEntry[], labels: SessionActionLabels): SessionActivity[] {
    const sessionId = session.sessionId.toString();
    return entries
        .filter(entry => (entry.session ?? '').toString() === sessionId)
        .flatMap(entry => {
            const key = actionLabelKeys[entry.kind];
            return key ? [{ entryId: entry.entryId.toString(), text: format(labels[key], { text: entry.text ?? '' }) }] : [];
        });
}
