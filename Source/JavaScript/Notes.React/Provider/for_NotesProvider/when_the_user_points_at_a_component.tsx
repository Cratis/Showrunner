// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button } from '@cratis/components/Common';
import { GiveNoteOnComponent } from '@cratis/showrunner.notes';
import { cleanup, fireEvent, render, RenderResult, waitFor } from '@testing-library/react';
import sinon from 'sinon';
import { NotesButton } from '../../Buttons/NotesButton.js';
import { NotesApplication, stubAvailability } from '../../given/NotesApplication.js';
import { stubExecute } from '../../given/executing.js';
import { press } from '../../given/pressing.js';

describe('when the user points at a component', () => {
    let result: RenderResult;
    let execution: ReturnType<typeof stubExecute>;

    beforeEach(async () => {
        stubAvailability(true);
        execution = stubExecute(GiveNoteOnComponent);
        result = render(
            <NotesApplication>
                <NotesButton />
                <div data-cratis-module='Orders' data-cratis-feature='Editing' data-cratis-slice='Change order' style={{ display: 'contents' }}>
                    <Button id='save-order' label='Save order' />
                </div>
            </NotesApplication>
        );
        fireEvent.click(result.getByRole('button', { name: 'More ways to give feedback' }));
        fireEvent.click(result.getByRole('menuitem', { name: 'Point at something…' }));
        await waitFor(() => (result.getByRole('button', { name: 'More ways to give feedback' }) as HTMLButtonElement).getAttribute('aria-expanded')!.should.equal('false'));
        fireEvent.click(result.getByRole('button', { name: 'Save order' }));
        await waitFor(() => result.getByRole('heading', { name: 'Give feedback on a component' }));
        fireEvent.change(result.getByLabelText('What do you think?'), { target: { value: 'Unclear label' } });
        await press(result, 'Send feedback');
        await waitFor(() => execution.stub.called.should.be.true);
    });

    afterEach(() => { cleanup(); sinon.restore(); });

    it('should send the component that was picked', () => (execution.executions[0].component as { componentName: string }).componentName.should.equal('Button'));
    it('should send the id of the component', () => (execution.executions[0].component as { componentId: string }).componentId.should.equal('save-order'));
    it('should send the slice of the closest marker', () => (execution.executions[0].component as { location: { slice: string } }).location.slice.should.equal('Change order'));
    it('should send the context', () => (execution.executions[0].context as { screen: { screenId: string } }).screen.screenId.should.equal('order-details'));
    it('should send the text', () => execution.executions[0].text!.should.equal('Unclear label'));
});
