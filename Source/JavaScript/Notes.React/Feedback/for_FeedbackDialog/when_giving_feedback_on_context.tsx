// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogResult } from '@cratis/arc.react/dialogs';
import { GiveNoteOnContext, NoteKind } from '@cratis/showrunner.notes';
import { cleanup, fireEvent, render, RenderResult, waitFor } from '@testing-library/react';
import sinon from 'sinon';
import { aNoteContext } from '../../given/aNoteContext.js';
import { press } from '../../given/pressing.js';
import { stubExecute } from '../../given/executing.js';
import { NotesHarness } from '../../given/NotesHarness.js';
import { FeedbackDialog } from '../FeedbackDialog.js';

describe('when giving feedback on the context', () => {
    const context = aNoteContext();
    let execution: ReturnType<typeof stubExecute>;
    let closeDialog: sinon.SinonStub;
    let result: RenderResult;

    beforeEach(async () => {
        execution = stubExecute(GiveNoteOnContext);
        closeDialog = sinon.stub();
        result = render(<NotesHarness closeDialog={closeDialog}><FeedbackDialog context={context} /></NotesHarness>);
        fireEvent.click(result.getByLabelText('Problem'));
        fireEvent.change(result.getByLabelText('What do you think?'), { target: { value: 'The save button is hard to find' } });
        await press(result, 'Send feedback', 2);
        await waitFor(() => closeDialog.called.should.be.true);
    });

    afterEach(() => { cleanup(); sinon.restore(); });

    it('should have the title', () => (result.queryByRole('heading', { name: 'Give feedback on this' }) === null).should.be.false);
    it('should say what was captured in plain language', () => result.getByText('Orders › Editing › Change order, on screen Order details').textContent!.should.contain('Change order'));
    it('should send the command once even when the button is pressed twice', () => execution.stub.callCount.should.equal(1));
    it('should send the context', () => (execution.executions[0].context === context).should.be.true);
    it('should send the kind', () => execution.executions[0].kind!.should.equal(NoteKind.problem));
    it('should send the text', () => execution.executions[0].text!.should.equal('The save button is hard to find'));
    it('should send a note id', () => (execution.executions[0].noteId !== undefined).should.be.true);
    it('should close the dialog', () => closeDialog.firstCall.args[0].should.equal(DialogResult.Ok));
});
