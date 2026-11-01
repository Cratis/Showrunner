// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogHeaderActionsRenderer, DialogHeaderContext } from '@cratis/components/Dialogs';
import { useNotesContext } from '../Provider/NotesContext.js';
import { NotesButton } from './NotesButton.js';

/**
 * Decides whether a dialog gets the feedback button in its header: only a dialog that has a slice location, and only when
 * `<NotesProvider enabled>` turned it on.
 * @param dialog The dialog.
 * @param enabled Whether the provider turned the header button on.
 * @returns True when the dialog shows the button.
 */
export function showsFeedbackButton(dialog: DialogHeaderContext, enabled: boolean): boolean {
    const location = dialog.sliceLocation;
    return enabled && location !== undefined && (!!location.module || !!location.feature || !!location.slice);
}

const NotesDialogHeaderButton = ({ dialog }: { dialog: DialogHeaderContext }) => {
    const { showsInDialogHeaders } = useNotesContext();
    return showsFeedbackButton(dialog, showsInDialogHeaders)
        ? <NotesButton size='small' dialog={dialog} />
        : null;
};

/**
 * The renderer to pass to `CratisComponentsProvider` as `dialogHeaderActions`: it puts the feedback button in the header of
 * every dialog that belongs to a slice. The context it captures is the dialog's location.
 * @param dialog What the dialog tells about itself.
 * @returns The button, or nothing for a dialog without a slice.
 */
export const notesDialogHeaderActions: DialogHeaderActionsRenderer = dialog => <NotesDialogHeaderButton dialog={dialog} />;
