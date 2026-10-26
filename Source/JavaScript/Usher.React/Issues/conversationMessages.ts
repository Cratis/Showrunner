// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ChatAuthor, ChatAuthorKind, ChatIdentifier, ChatMessage } from '@cratis/components/Chat';
import { Guid } from '@cratis/fundamentals';
import { ConversationAuthorKind, ConversationEntry, ConversationEntryKind } from '@cratis/showrunner.usher';
import { ConversationEventLabels, format, UsherLabels } from '../Labels/index.js';

/**
 * A message in the conversation on an issue, as the chat shows it.
 */
export interface ConversationMessage extends ChatMessage {
    /** What the entry records. */
    readonly kind: ConversationEntryKind;

    /** The question a question or an answer is about; empty for other entries. */
    readonly questionId: Guid;
}

const textKinds = new Set<ConversationEntryKind>([
    ConversationEntryKind.errorReported,
    ConversationEntryKind.issueDescribed,
    ConversationEntryKind.detailsAdded,
    ConversationEntryKind.questionAsked,
    ConversationEntryKind.questionAnswered,
    ConversationEntryKind.replied
]);

const eventLabelKeys: Partial<Record<ConversationEntryKind, keyof ConversationEventLabels>> = {
    [ConversationEntryKind.identityShared]: 'identityShared',
    [ConversationEntryKind.accessGranted]: 'accessGranted',
    [ConversationEntryKind.accessRevoked]: 'accessRevoked',
    [ConversationEntryKind.accessLapsed]: 'accessLapsed',
    [ConversationEntryKind.accessRequested]: 'accessRequested',
    [ConversationEntryKind.supportSignedIn]: 'supportSignedIn',
    [ConversationEntryKind.supportSessionEnded]: 'supportSessionEnded',
    [ConversationEntryKind.triaged]: 'triaged',
    [ConversationEntryKind.accepted]: 'accepted',
    [ConversationEntryKind.declinedToFix]: 'declinedToFix',
    [ConversationEntryKind.workStarted]: 'workStarted',
    [ConversationEntryKind.resolved]: 'resolved',
    [ConversationEntryKind.reopened]: 'reopened',
    [ConversationEntryKind.withdrawn]: 'withdrawn',
    [ConversationEntryKind.linkedToKnownIssue]: 'linkedToKnownIssue',
    [ConversationEntryKind.unlinkedFromKnownIssue]: 'unlinkedFromKnownIssue',
    [ConversationEntryKind.linkedToWorkItem]: 'linkedToWorkItem',
    [ConversationEntryKind.unlinkedFromWorkItem]: 'unlinkedFromWorkItem',
    [ConversationEntryKind.fixReleased]: 'fixReleased'
};

const authorKindNames: Record<ConversationAuthorKind, string> = {
    [ConversationAuthorKind.reporter]: 'reporter',
    [ConversationAuthorKind.support]: 'support',
    [ConversationAuthorKind.agent]: 'agent',
    [ConversationAuthorKind.system]: 'system'
};

/**
 * Gets the author id a conversation entry is shown under: who it is from and, for support and agents, their name.
 * @param authorKind Who the entry is from.
 * @param authorName The name of the support person or agent; empty when the desk hides it.
 * @returns The author id.
 */
export function authorIdFor(authorKind: ConversationAuthorKind, authorName: string): string {
    const kind = authorKindNames[authorKind] ?? authorKindNames[ConversationAuthorKind.system];
    return authorKind === ConversationAuthorKind.support || authorKind === ConversationAuthorKind.agent
        ? `${kind}:${authorName ?? ''}`
        : kind;
}

/**
 * Resolves an author id of the conversation to who the chat shows.
 * @param authorId The author id, as {@link authorIdFor} gives it.
 * @param labels The labels to show.
 * @returns The {@link ChatAuthor}.
 */
export function authorOf(authorId: ChatIdentifier, labels: UsherLabels): ChatAuthor {
    const id = authorId.toString();
    const separator = id.indexOf(':');
    const kind = separator >= 0 ? id.substring(0, separator) : id;
    const name = separator >= 0 ? id.substring(separator + 1) : '';

    switch (kind) {
        case 'reporter':
            return { name: labels.conversation.you, kind: ChatAuthorKind.User };
        case 'support':
            return { name: name.length > 0 ? name : labels.conversation.support, kind: ChatAuthorKind.User };
        case 'agent':
            return { name: name.length > 0 ? name : labels.conversation.support, kind: ChatAuthorKind.Agent };
        default:
            return { name: labels.conversation.update, kind: ChatAuthorKind.Agent };
    }
}

/**
 * Gets what the chat shows for an entry: its text for what someone wrote, and a sentence for what happened.
 * @param entry The {@link ConversationEntry}.
 * @param labels The labels to show.
 * @returns The body of the message.
 */
export function bodyOf(entry: ConversationEntry, labels: UsherLabels): string {
    if (textKinds.has(entry.kind)) {
        return entry.text;
    }

    const key = eventLabelKeys[entry.kind];
    return key ? format(labels.conversation.events[key], { text: entry.text ?? '' }) : entry.text;
}

/**
 * Converts the entries of the conversation on an issue to the messages the chat shows.
 * @param issueId The issue.
 * @param entries The entries, oldest first.
 * @param labels The labels to show.
 * @returns The {@link ConversationMessage} for every entry, oldest first.
 */
export function conversationMessages(issueId: Guid, entries: ConversationEntry[], labels: UsherLabels): ConversationMessage[] {
    return entries.map(entry => ({
        id: `${entry.entryId}`,
        topicId: issueId,
        authorId: authorIdFor(entry.authorKind, entry.authorName),
        body: bodyOf(entry, labels),
        timestamp: entry.at instanceof Date ? entry.at : new Date(entry.at),
        kind: entry.kind,
        questionId: entry.questionId ?? Guid.empty
    }));
}

/**
 * Checks whether a question is open: there is one, and the reporter has not answered it.
 * @param questionId The question; empty when there is none.
 * @returns True when there is a question to answer.
 */
export function isOpenQuestion(questionId: Guid | undefined): questionId is Guid {
    return questionId !== undefined && !Guid.empty.equals(questionId);
}
