// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { GiveNoteOnSlices } from '@cratis/showrunner.notes';
import { cleanup, fireEvent, render, RenderResult, waitFor } from '@testing-library/react';
import sinon from 'sinon';
import { aNoteContext } from '../../given/aNoteContext.js';
import { stubExecute } from '../../given/executing.js';
import { NotesHarness } from '../../given/NotesHarness.js';
import { press } from '../../given/pressing.js';
import { SlicesFeedbackDialog } from '../SlicesFeedbackDialog.js';

describe('when giving feedback on slices', () => {
    const context = aNoteContext();
    const known = [
        { module: 'Billing', feature: 'Invoices', slice: 'Pay' },
        { module: 'Orders', feature: 'Editing', slice: 'Change order' },
        { module: 'Shipping', feature: 'Labels', slice: 'Print label' }
    ];
    let execution: ReturnType<typeof stubExecute>;
    let result: RenderResult;

    beforeEach(async () => {
        execution = stubExecute(GiveNoteOnSlices);
        result = render(<NotesHarness><SlicesFeedbackDialog context={context} knownSlices={known} /></NotesHarness>);
        fireEvent.click(result.getByRole('checkbox', { name: 'Orders › Listing › Order list' }));
        fireEvent.change(result.getByLabelText('Search all slices'), { target: { value: 'bill' } });
        fireEvent.click(result.getByRole('checkbox', { name: 'Billing › Invoices › Pay' }));
        fireEvent.change(result.getByLabelText('What do you think?'), { target: { value: 'These two should work together' } });
        await press(result, 'Send feedback');
        await waitFor(() => execution.stub.called.should.be.true);
    });

    afterEach(() => { cleanup(); sinon.restore(); });

    it('should list the slices active on the screen', () => result.getByRole('checkbox', { name: 'Orders › Editing › Change order' }).tagName.should.equal('INPUT'));
    it('should not list a slice that does not match the search', () => (result.queryByRole('checkbox', { name: 'Shipping › Labels › Print label' }) === null).should.be.true);
    it('should send the chosen slices', () => (execution.executions[0].slices as Array<{ slice: string }>).map(slice => slice.slice).should.deep.equal(['Order list', 'Pay']));
    it('should send the context', () => (execution.executions[0].context === context).should.be.true);
    it('should send the text', () => execution.executions[0].text!.should.equal('These two should work together'));
});
