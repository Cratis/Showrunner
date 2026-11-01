// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { QueryResultWithState } from '@cratis/arc/queries';
import { MyNote, MyNotes, NoteStatus, WithdrawNote } from '@cratis/showrunner.notes';
import { cleanup, fireEvent, render, RenderResult, waitFor } from '@testing-library/react';
import sinon from 'sinon';
import { CommandResult } from '@cratis/arc/commands';
import { NotesHarness } from '../../given/NotesHarness.js';
import { stubExecute } from '../../given/executing.js';
import { MyFeedbackDialog } from '../MyFeedbackDialog.js';

describe('when withdrawing a note', () => {
    const noteId = Guid.create();
    let execution: ReturnType<typeof stubExecute>;
    let release: () => void;
    let result: RenderResult;

    beforeEach(async () => {
        const note = Object.assign(new MyNote(), {
            id: noteId, text: 'Too many clicks', status: NoteStatus.given, statusDetail: '', target: 0,
            location: { module: 'Orders', feature: '', slice: '' }, screen: { screenId: '', screenName: '', screenPath: '' },
            slices: [], component: '', propertyChanges: [], lastActivityAt: new Date()
        });
        sinon.stub(MyNotes, 'use').returns([QueryResultWithState.empty([note]), (() => undefined) as never]);
        execution = stubExecute(WithdrawNote, () => new Promise<CommandResult>(resolve => { release = () => resolve(CommandResult.empty); }));
        result = render(<NotesHarness><MyFeedbackDialog /></NotesHarness>);
        const withdraw = result.getByRole('button', { name: 'Withdraw the note: Too many clicks' });
        fireEvent.click(withdraw);
        fireEvent.click(withdraw);
        await waitFor(() => execution.stub.called.should.be.true);
    });

    afterEach(() => { release?.(); cleanup(); sinon.restore(); });

    it('should send the withdrawal once even when pressed twice', () => execution.stub.callCount.should.equal(1));
    it('should withdraw the note', () => (execution.executions[0].noteId as Guid).toString().should.equal(noteId.toString()));
    it('should switch the button off while it is in flight', () => (result.getByRole('button', { name: 'Withdraw the note: Too many clicks' }) as HTMLButtonElement).disabled.should.be.true);
});
