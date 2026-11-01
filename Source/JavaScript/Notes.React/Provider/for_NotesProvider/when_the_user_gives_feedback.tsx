// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { QueryResultWithState } from '@cratis/arc/queries';
import { GiveNoteOnContext, GiveNoteOnScreen, MyNotes } from '@cratis/showrunner.notes';
import { cleanup, fireEvent, render, RenderResult, waitFor } from '@testing-library/react';
import sinon from 'sinon';
import { NotesButton } from '../../Buttons/NotesButton.js';
import { NotesApplication, stubAvailability } from '../../given/NotesApplication.js';
import { stubExecute } from '../../given/executing.js';
import { press } from '../../given/pressing.js';

describe('when the user gives feedback', () => {
    afterEach(() => { cleanup(); sinon.restore(); });

    describe('and notes is on for a signed in user', () => {
        let result: RenderResult;

        beforeEach(() => {
            stubAvailability(true);
            result = render(<NotesApplication><NotesButton /></NotesApplication>);
        });

        describe('and the main action is clicked', () => {
            let execution: ReturnType<typeof stubExecute>;

            beforeEach(async () => {
                execution = stubExecute(GiveNoteOnContext);
                window.history.pushState({}, '', '/orders/42');
                fireEvent.click(result.getByRole('button', { name: 'Give feedback' }));
                await waitFor(() => result.getByLabelText('What do you think?'));
                fireEvent.change(result.getByLabelText('What do you think?'), { target: { value: 'Hard to find' } });
                await press(result, 'Send feedback');
                await waitFor(() => execution.stub.called.should.be.true);
            });

            it('should send the screen of the application context', () => (execution.executions[0].context as { screen: { screenName: string } }).screen.screenName.should.equal('Order details'));
            it('should send the route', () => (execution.executions[0].context as { route: string }).route.should.equal('/orders/42'));
            it('should send what the host contributed as model references', () => (execution.executions[0].context as { modelReferences: Record<string, string> }).modelReferences.should.deep.equal({ eventModel: 'orders-v3' }));
            it('should send a correlation id', () => (execution.executions[0].context as { correlationId: { toString(): string } }).correlationId.toString().should.not.equal('00000000-0000-0000-0000-000000000000'));
            it('should send the text', () => execution.executions[0].text!.should.equal('Hard to find'));
        });

        describe('and the drop-down asks about the screen', () => {
            let execution: ReturnType<typeof stubExecute>;

            beforeEach(async () => {
                execution = stubExecute(GiveNoteOnScreen);
                fireEvent.click(result.getByRole('button', { name: 'More ways to give feedback' }));
                fireEvent.click(result.getByRole('menuitem', { name: 'About this screen…' }));
                await waitFor(() => result.getByLabelText('What do you think?'));
                fireEvent.change(result.getByLabelText('What do you think?'), { target: { value: 'Cluttered' } });
                await press(result, 'Send feedback');
                await waitFor(() => execution.stub.called.should.be.true);
            });

            it('should send the screen note with the context', () => (execution.executions[0].context as { screen: { screenId: string } }).screen.screenId.should.equal('order-details'));
        });

        describe('and the drop-down opens my feedback', () => {
            it('should open the side sheet', async () => {
                sinon.stub(MyNotes, 'use').returns([QueryResultWithState.empty([]), (() => undefined) as never]);
                fireEvent.click(result.getByRole('button', { name: 'More ways to give feedback' }));
                fireEvent.click(result.getByRole('menuitem', { name: 'My feedback' }));
                await waitFor(() => result.getByRole('heading', { name: 'My feedback' }));
            });
        });
    });

    describe('and the user is not signed in', () => {
        it('should render nothing', () => {
            stubAvailability(true);
            const result = render(<NotesApplication signedIn={false}><NotesButton /></NotesApplication>);
            (result.queryByRole('button', { name: 'Give feedback' }) === null).should.equal(true);
        });
    });

    describe('and notes is switched off', () => {
        it('should render nothing', () => {
            stubAvailability(false);
            const result = render(<NotesApplication><NotesButton /></NotesApplication>);
            (result.queryByRole('button', { name: 'Give feedback' }) === null).should.equal(true);
        });
    });
});
