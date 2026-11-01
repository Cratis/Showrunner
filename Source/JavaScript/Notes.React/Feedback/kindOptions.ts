// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { NoteKind } from '@cratis/showrunner.notes';
import { NotesLabels } from '../Labels/index.js';

/**
 * One choice of the kind field.
 */
export type KindOption = {
    /** The kind. */
    readonly value: NoteKind;

    /** The label. */
    readonly label: string;
};

/**
 * Gets the kinds of feedback a user can choose from.
 * @param labels The labels to show.
 * @returns The {@link KindOption}s, in the order they are offered.
 */
export function kindOptions(labels: NotesLabels): KindOption[] {
    return [
        { value: NoteKind.problem, label: labels.kinds.problem },
        { value: NoteKind.suggestion, label: labels.kinds.suggestion },
        { value: NoteKind.question, label: labels.kinds.question },
        { value: NoteKind.praise, label: labels.kinds.praise }
    ];
}

/**
 * The kind a note starts as.
 */
export const defaultKind: NoteKind = NoteKind.suggestion;
