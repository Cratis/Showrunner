// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { GiveNoteOnScreen } from '@cratis/showrunner.notes';
import { cleanup, fireEvent, render, RenderResult, waitFor } from '@testing-library/react';
import sinon from 'sinon';
import { aNoteContext } from '../../given/aNoteContext.js';
import { press } from '../../given/pressing.js';
import { stubExecute } from '../../given/executing.js';
import { NotesHarness } from '../../given/NotesHarness.js';
import { FeedbackDialog } from '../FeedbackDialog.js';

describe('when giving feedback on the screen', () => {
    const context = aNoteContext();
    let execution: ReturnType<typeof stubExecute>;
    let result: RenderResult;

    beforeEach(async () => {
        execution = stubExecute(GiveNoteOnScreen);
        result = render(<NotesHarness><FeedbackDialog context={context} about='screen' /></NotesHarness>);
        fireEvent.change(result.getByLabelText('What do you think?'), { target: { value: 'This screen is cluttered' } });
        await press(result, 'Send feedback');
        await waitFor(() => execution.stub.called.should.be.true);
    });

    afterEach(() => { cleanup(); sinon.restore(); });

    it('should have the screen title', () => (result.queryByRole('heading', { name: 'Give feedback on this screen' }) === null).should.be.false);
    it('should say the screen', () => result.getByText('Screen Order details').textContent!.should.contain('Order details'));
    it('should send the context', () => (execution.executions[0].context === context).should.be.true);
    it('should send the text', () => execution.executions[0].text!.should.equal('This screen is cluttered'));
});
