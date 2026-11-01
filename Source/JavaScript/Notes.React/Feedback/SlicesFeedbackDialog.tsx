// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogResult, useDialogContext } from '@cratis/arc.react/dialogs';
import { CommandDialog } from '@cratis/components/CommandDialog';
import { DialogInitialFocus } from '@cratis/components/Dialogs';
import { Guid } from '@cratis/fundamentals';
import { GiveNoteOnSlices, NoteContext } from '@cratis/showrunner.notes';
import { useMemo, useState } from 'react';
import { describeContext, toSliceLocation } from '../Context/index.js';
import { LocationParts } from '../Context/formatLocation.js';
import { useNotesContext } from '../Provider/NotesContext.js';
import { ContextSummary } from './ContextSummary.js';
import { FeedbackGiven } from './FeedbackDialog.js';
import { defaultKind } from './kindOptions.js';
import { KindAndText } from './KindAndText.js';
import { SliceChooser } from './SliceChooser.js';
import { distinctSlices } from './sliceChoice.js';

/**
 * Props for {@link SlicesFeedbackDialog}.
 */
export interface SlicesFeedbackDialogProps {
    /** What the user was looking at when they chose to give feedback. */
    context: NoteContext;

    /** Every slice the host knows about, to search over. */
    knownSlices?: readonly LocationParts[];
}

/**
 * The give feedback on specific slices dialog: the user chooses among the slices active on the screen and searches
 * all the slices the host knows. It sends `GiveNoteOnSlices` and closes with {@link FeedbackGiven}. Open it with
 * `useDialog`.
 */
export const SlicesFeedbackDialog = ({ context, knownSlices = [] }: SlicesFeedbackDialogProps) => {
    const { labels } = useNotesContext();
    const { closeDialog } = useDialogContext<SlicesFeedbackDialogProps, FeedbackGiven>();
    const [noteId] = useState(() => Guid.create());
    const active = useMemo(() => distinctSlices([context.location, ...context.activeSlices]), [context]);
    const [chosen, setChosen] = useState<LocationParts[]>([]);
    const currentValues = useMemo(() => ({ slices: chosen.map(toSliceLocation) }), [chosen]);

    return (
        <CommandDialog<GiveNoteOnSlices>
            command={GiveNoteOnSlices}
            title={labels.slices.title}
            okLabel={labels.feedback.send}
            cancelLabel={labels.common.cancel}
            closeAriaLabel={labels.common.close}
            width='640px'
            initialFocus={DialogInitialFocus.Content}
            initialValues={{ context, noteId, kind: defaultKind }}
            currentValues={currentValues}
            onSuccess={() => closeDialog(DialogResult.Ok, { noteId })}>
            <div className='notes-feedback-form'>
                <ContextSummary text={describeContext(context, labels)} />
                <SliceChooser active={active} known={knownSlices} chosen={chosen} onChange={setChosen} />
                <KindAndText<GiveNoteOnSlices> />
            </div>
        </CommandDialog>
    );
};
