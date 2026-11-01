// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CratisComponentsProvider } from '@cratis/components';
import { Dialog } from '@cratis/components/Dialogs';
import { cleanup, render } from '@testing-library/react';
import { defaultNotesContextValue, NotesContext } from '../../Provider/NotesContext.js';
import { notesDialogHeaderActions, showsFeedbackButton } from '../notesDialogHeaderActions.js';

const renderDialog = (enabled: boolean, sliceLocation?: { module: string; feature: string; slice: string }) => render(
    <CratisComponentsProvider dialogHeaderActions={notesDialogHeaderActions}>
        <NotesContext.Provider value={{ ...defaultNotesContextValue, isProvided: true, isAvailable: true, showsInDialogHeaders: enabled }}>
            <Dialog title='Pay invoice' sliceLocation={sliceLocation}>Content</Dialog>
        </NotesContext.Provider>
    </CratisComponentsProvider>
);

describe('when opening dialogs', () => {
    afterEach(() => cleanup());

    it('should add the button to the header of a dialog with a slice', () => {
        renderDialog(true, { module: 'Billing', feature: 'Invoices', slice: 'Pay' });
        (document.querySelector('.cratis-dialog [aria-label="Give feedback"]') !== null).should.equal(true);
    });

    it('should not add the button to a dialog without a slice', () => {
        renderDialog(true);
        (document.querySelector('[aria-label="Give feedback"]') === null).should.equal(true);
    });

    it('should not add the button to a dialog with an empty slice', () => {
        renderDialog(true, { module: '', feature: '', slice: '' });
        (document.querySelector('[aria-label="Give feedback"]') === null).should.equal(true);
    });

    it('should not add the button unless the provider is enabled', () => {
        renderDialog(false, { module: 'Billing', feature: 'Invoices', slice: 'Pay' });
        (document.querySelector('[aria-label="Give feedback"]') === null).should.equal(true);
    });

    it('should decide from the slice of the dialog', () =>
        [showsFeedbackButton({ dialogId: 'd', title: 't', isCommandDialog: false }, true), showsFeedbackButton({ dialogId: 'd', title: 't', isCommandDialog: true, sliceLocation: { module: 'A', feature: '', slice: '' } }, true)].should.deep.equal([false, true]));
});
