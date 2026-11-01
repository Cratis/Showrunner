// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Dialog, DialogInitialFocus } from '@cratis/components/Dialogs';
import { MyNote, MyNotes, WithdrawNote } from '@cratis/showrunner.notes';
import { useRef, useState } from 'react';
import { useNotesLabels } from '../Provider/NotesContext.js';
import { MyFeedbackList } from './MyFeedbackList.js';

/**
 * The my feedback side sheet: the user's notes with their status, kept current as the receiving side acknowledges,
 * addresses or declines them. Open it with `useDialog`.
 */
export const MyFeedbackDialog = () => {
    const labels = useNotesLabels();
    const [result] = MyNotes.use();
    const [withdraw, setWithdraw] = WithdrawNote.use();
    const inFlight = useRef(new Set<string>());
    const [withdrawing, setWithdrawing] = useState<ReadonlySet<string>>(new Set());

    const onWithdraw = async (note: MyNote) => {
        const id = note.id.toString();
        if (inFlight.current.has(id)) {
            return;
        }

        inFlight.current.add(id);
        setWithdrawing(new Set(inFlight.current));
        try {
            setWithdraw({ noteId: note.id });
            await withdraw.execute();
        } finally {
            inFlight.current.delete(id);
            setWithdrawing(new Set(inFlight.current));
        }
    };

    return (
        <Dialog
            title={labels.myFeedback.title}
            subtitle={labels.myFeedback.description}
            placement='end'
            width='640px'
            buttons={null}
            dismissable
            initialFocus={DialogInitialFocus.Content}
            closeAriaLabel={labels.common.close}>
            <MyFeedbackList notes={result.data ?? []} labels={labels} withdrawing={withdrawing} onWithdraw={onWithdraw} />
        </Dialog>
    );
};
