// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogContext, DialogContextContent } from '@cratis/arc.react/dialogs';
import { CratisComponentsProvider } from '@cratis/components';
import { ReactNode } from 'react';
import sinon from 'sinon';
import { NotesContext, NotesContextValue, defaultNotesContextValue } from '../Provider/NotesContext.js';

export interface NotesHarnessProps {
    notes?: Partial<NotesContextValue>;
    closeDialog?: sinon.SinonStub;
    children?: ReactNode;
}

export const NotesHarness = ({ notes, closeDialog = sinon.stub(), children }: NotesHarnessProps) => (
    <CratisComponentsProvider>
        <NotesContext.Provider value={{ ...defaultNotesContextValue, isProvided: true, isAvailable: true, ...notes }}>
            <DialogContext.Provider value={new DialogContextContent({}, closeDialog)}>
                {children}
            </DialogContext.Provider>
        </NotesContext.Provider>
    </CratisComponentsProvider>
);
