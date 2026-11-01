// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { MyNote, NoteStatus, NoteTarget } from '@cratis/showrunner.notes';
import { formatLocation } from '../Context/formatLocation.js';
import { format, NotesLabels } from '../Labels/index.js';

/**
 * Says where a note stands: given, acknowledged, addressed in a version, declined with a reason, or withdrawn.
 * @param note The note.
 * @param labels The labels to show.
 * @returns The status in plain language.
 */
export function statusText(note: Pick<MyNote, 'status' | 'statusDetail'>, labels: NotesLabels): string {
    const statuses = labels.myFeedback.statuses;
    switch (note.status) {
        case NoteStatus.acknowledged: return statuses.acknowledged;
        case NoteStatus.addressed: return format(statuses.addressed, { version: note.statusDetail });
        case NoteStatus.declined: return format(statuses.declined, { reason: note.statusDetail });
        case NoteStatus.withdrawn: return statuses.withdrawn;
        default: return statuses.given;
    }
}

/**
 * Says what a note is about.
 * @param note The note.
 * @param labels The labels to show.
 * @returns The subject in plain language.
 */
export function targetText(note: Pick<MyNote, 'target' | 'location' | 'screen' | 'slices' | 'component'>, labels: NotesLabels): string {
    const targets = labels.myFeedback.targets;
    const location = (parts: Parameters<typeof formatLocation>[0]) => formatLocation(parts, labels.common.locationSeparator, labels.common.unknownLocation);
    switch (note.target) {
        case NoteTarget.slices: return `${targets.slices}: ${note.slices.map(location).join(', ')}`;
        case NoteTarget.screen: return `${targets.screen}: ${note.screen.screenName || note.screen.screenPath || labels.common.unknownLocation}`;
        case NoteTarget.component: return `${format(targets.component, { component: note.component })} · ${location(note.location)}`;
        default: return `${targets.context}: ${location(note.location)}`;
    }
}

/**
 * Gets what to show of what the user wrote.
 * @param note The note.
 * @param labels The labels to show.
 * @returns The text; the count of suggested property changes for a note that has no text.
 */
export function noteText(note: Pick<MyNote, 'text' | 'propertyChanges'>, labels: NotesLabels): string {
    if (note.text) {
        return note.text;
    }

    return note.propertyChanges.length > 0
        ? format(labels.myFeedback.propertyChanges, { count: note.propertyChanges.length })
        : labels.myFeedback.suggestionsOnly;
}

/**
 * Whether the user can still withdraw a note: once the receiving side has answered it, or it is withdrawn, there is
 * nothing to withdraw.
 * @param note The note.
 * @returns True when the note can be withdrawn.
 */
export function canWithdraw(note: Pick<MyNote, 'status'>): boolean {
    return note.status === NoteStatus.given || note.status === NoteStatus.acknowledged;
}

/**
 * Sorts the notes the way the user wants to read them: most recent activity first.
 * @param notes The notes.
 * @returns A new list, most recent activity first.
 */
export function newestFirst<TNote extends Pick<MyNote, 'lastActivityAt'>>(notes: readonly TNote[]): TNote[] {
    return [...notes].sort((a, b) => new Date(b.lastActivityAt).getTime() - new Date(a.lastActivityAt).getTime());
}
