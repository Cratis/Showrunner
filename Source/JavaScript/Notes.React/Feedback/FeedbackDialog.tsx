// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogResult, useDialogContext } from '@cratis/arc.react/dialogs';
import { CommandDialog } from '@cratis/components/CommandDialog';
import { DialogInitialFocus } from '@cratis/components/Dialogs';
import { Guid } from '@cratis/fundamentals';
import { GiveNoteOnContext, GiveNoteOnScreen, NoteContext } from '@cratis/showrunner.notes';
import { useState } from 'react';
import { describeContext, describeScreen } from '../Context/index.js';
import { useNotesContext } from '../Provider/NotesContext.js';
import { ContextSummary } from './ContextSummary.js';
import { defaultKind } from './kindOptions.js';
import { KindAndText } from './KindAndText.js';

/**
 * What the feedback dialogs close with.
 */
export interface FeedbackGiven {
    /** The id of the note. */
    readonly noteId: Guid;
}

/**
 * Props for {@link FeedbackDialog}.
 */
export interface FeedbackDialogProps {
    /** What the user was looking at when they chose to give feedback. */
    context: NoteContext;

    /** Whether the feedback is about what was on screen, or about the screen. Defaults to what was on screen. */
    about?: 'context' | 'screen';
}

/**
 * The give feedback dialog: shows in plain language what was captured, and takes the kind and the text. It sends
 * `GiveNoteOnContext`, or `GiveNoteOnScreen` for a screen, and closes with {@link FeedbackGiven}. Open it with
 * `useDialog`.
 */
export const FeedbackDialog = ({ context, about = 'context' }: FeedbackDialogProps) => {
    const { labels } = useNotesContext();
    const { closeDialog } = useDialogContext<FeedbackDialogProps, FeedbackGiven>();
    const [noteId] = useState(() => Guid.create());
    const isScreen = about === 'screen';
    const command = isScreen ? GiveNoteOnScreen : GiveNoteOnContext;

    return (
        <CommandDialog<GiveNoteOnContext | GiveNoteOnScreen>
            command={command as typeof GiveNoteOnContext}
            title={isScreen ? labels.feedback.screenTitle : labels.feedback.contextTitle}
            okLabel={labels.feedback.send}
            cancelLabel={labels.common.cancel}
            closeAriaLabel={labels.common.close}
            width='560px'
            initialFocus={DialogInitialFocus.Content}
            initialValues={{ context, noteId, kind: defaultKind }}
            onSuccess={() => closeDialog(DialogResult.Ok, { noteId })}>
            <div className='notes-feedback-form'>
                <ContextSummary text={isScreen ? describeScreen(context, labels) : describeContext(context, labels)} />
                <KindAndText<GiveNoteOnContext> />
            </div>
        </CommandDialog>
    );
};
