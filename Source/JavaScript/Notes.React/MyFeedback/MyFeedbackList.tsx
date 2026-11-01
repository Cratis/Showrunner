// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button } from '@cratis/components/Common';
import { Guid } from '@cratis/fundamentals';
import { MyNote, NoteStatus } from '@cratis/showrunner.notes';
import { format, NotesLabels } from '../Labels/index.js';
import { canWithdraw, newestFirst, noteText, statusText, targetText } from './noteStates.js';

/**
 * Props for {@link MyFeedbackList}.
 */
export interface MyFeedbackListProps {
    /** The user's notes. */
    notes: readonly MyNote[];

    /** The labels to show. */
    labels: NotesLabels;

    /** The notes being withdrawn right now. */
    withdrawing?: ReadonlySet<string>;

    /** Called with the note the user chooses to withdraw. */
    onWithdraw: (note: MyNote) => void;
}

/**
 * Lists the user's notes with where each stands, and a button to withdraw the ones that can still be withdrawn.
 */
export const MyFeedbackList = ({ notes, labels, withdrawing = new Set(), onWithdraw }: MyFeedbackListProps) => {
    if (notes.length === 0) {
        return <p className='notes-my-feedback__empty'>{labels.myFeedback.empty}</p>;
    }

    return (
        <ul className='notes-my-feedback__list'>
            {newestFirst(notes).map(note => {
                const id = (note.id as Guid).toString();
                const text = noteText(note, labels);
                return (
                    <li key={id} className='notes-my-feedback__note' data-status={NoteStatus[note.status]}>
                        <div className='notes-my-feedback__header'>
                            <span className='notes-my-feedback__status'>{statusText(note, labels)}</span>
                            <span className='notes-my-feedback__target'>{targetText(note, labels)}</span>
                        </div>
                        <p className='notes-my-feedback__text'>{text}</p>
                        {canWithdraw(note) && (
                            <Button
                                variant='outline'
                                size='small'
                                label={labels.myFeedback.withdraw}
                                aria-label={format(labels.myFeedback.withdrawNote, { text: text.slice(0, 40) })}
                                disabled={withdrawing.has(id)}
                                loading={withdrawing.has(id)}
                                onClick={() => onWithdraw(note)} />
                        )}
                    </li>
                );
            })}
        </ul>
    );
};
