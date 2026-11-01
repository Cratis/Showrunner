// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { cleanup, fireEvent, render, RenderResult } from '@testing-library/react';
import sinon from 'sinon';
import { NotesHarness } from '../../given/NotesHarness.js';
import { NotesContextValue } from '../../Provider/NotesContext.js';
import { NotesButton, NotesButtonProps } from '../NotesButton.js';

const openers = () => ({
    giveFeedback: sinon.stub(),
    giveFeedbackOnSlices: sinon.stub(),
    giveFeedbackOnScreen: sinon.stub(),
    pointAtSomething: sinon.stub(),
    showMyFeedback: sinon.stub()
});

const renderButton = (props: NotesButtonProps, notes: Partial<NotesContextValue>): RenderResult => render(
    <NotesHarness notes={notes}>
        <NotesButton {...props} />
    </NotesHarness>
);

describe('when giving feedback', () => {
    afterEach(() => cleanup());

    describe('and the main action is clicked', () => {
        let given: ReturnType<typeof openers>;

        beforeEach(() => {
            given = openers();
            const result = renderButton({}, given);
            fireEvent.click(result.getByRole('button', { name: 'Give feedback' }));
        });

        it('should give feedback on the context', () => given.giveFeedback.calledOnce.should.be.true);
        it('should not open anything else', () => [given.giveFeedbackOnSlices, given.giveFeedbackOnScreen, given.pointAtSomething, given.showMyFeedback].every(opener => !opener.called).should.be.true);
    });

    describe('and the button is small', () => {
        let button: HTMLButtonElement;

        beforeEach(() => {
            button = renderButton({ size: 'small' }, openers()).getByRole('button', { name: 'Give feedback' }) as HTMLButtonElement;
        });

        it('should show no text', () => (button.textContent ?? '').trim().should.equal(''));
        it('should have a tooltip', () => button.hasAttribute('data-cratis-tooltip-trigger').should.be.true);
    });

    describe('and it is in a dialog', () => {
        const dialog = { dialogId: 'd1', title: 'Pay', isCommandDialog: true, sliceLocation: { module: 'Billing', feature: 'Invoices', slice: 'Pay' } };
        let given: ReturnType<typeof openers>;
        let result: RenderResult;

        beforeEach(() => {
            given = openers();
            result = renderButton({ size: 'small', dialog }, given);
        });

        it('should give the dialog with the main action', () => {
            fireEvent.click(result.getByRole('button', { name: 'Give feedback' }));
            given.giveFeedback.firstCall.args[0].should.equal(dialog);
        });

        it('should give the dialog with a drop-down item', () => {
            fireEvent.click(result.getByRole('button', { name: 'More ways to give feedback' }));
            fireEvent.click(result.getByRole('menuitem', { name: 'About this screen…' }));
            given.giveFeedbackOnScreen.firstCall.args[0].should.equal(dialog);
        });
    });

    describe('and the drop-down is opened', () => {
        let given: ReturnType<typeof openers>;
        let result: RenderResult;
        let toggle: HTMLElement;

        beforeEach(() => {
            given = openers();
            result = renderButton({}, given);
            toggle = result.getByRole('button', { name: 'More ways to give feedback' });
            fireEvent.click(toggle);
        });

        it('should say it has a menu', () => toggle.getAttribute('aria-haspopup')!.should.equal('menu'));
        it('should say it is expanded', () => toggle.getAttribute('aria-expanded')!.should.equal('true'));
        it('should offer the four items', () => result.getAllByRole('menuitem').map(item => item.textContent).should.deep.equal(['About specific slices…', 'About this screen…', 'Point at something…', 'My feedback']));
        it('should focus the first item', () => (document.activeElement as HTMLElement).textContent!.should.equal('About specific slices…'));

        it('should open feedback on slices', () => {
            fireEvent.click(result.getByRole('menuitem', { name: 'About specific slices…' }));
            given.giveFeedbackOnSlices.calledOnce.should.equal(true);
        });

        it('should open feedback on the screen', () => {
            fireEvent.click(result.getByRole('menuitem', { name: 'About this screen…' }));
            given.giveFeedbackOnScreen.calledOnce.should.equal(true);
        });

        it('should start pointing at something', () => {
            fireEvent.click(result.getByRole('menuitem', { name: 'Point at something…' }));
            given.pointAtSomething.calledOnce.should.equal(true);
        });

        it('should open my feedback', () => {
            fireEvent.click(result.getByRole('menuitem', { name: 'My feedback' }));
            given.showMyFeedback.calledOnce.should.equal(true);
        });

        it('should close after choosing', () => {
            fireEvent.click(result.getByRole('menuitem', { name: 'My feedback' }));
            (result.queryByRole('menu') === null).should.equal(true);
        });

        it('should move with the arrow keys', () => {
            fireEvent.keyDown(result.getByRole('menu'), { key: 'ArrowDown' });
            (document.activeElement as HTMLElement).textContent!.should.equal('About this screen…');
        });

        it('should close with escape and give focus back to the toggle', () => {
            fireEvent.keyDown(result.getByRole('menu'), { key: 'Escape' });
            ((result.queryByRole('menu') === null) && document.activeElement === toggle).should.equal(true);
        });
    });

    describe('and the user is already pointing at something', () => {
        it('should switch off pointing', () => {
            const result = renderButton({}, { ...openers(), isPicking: true });
            fireEvent.click(result.getByRole('button', { name: 'More ways to give feedback' }));
            (result.getByRole('menuitem', { name: 'Point at something…' }) as HTMLButtonElement).disabled.should.equal(true);
        });
    });

    describe('and notes is not available', () => {
        it('should render nothing', () => {
            const result = renderButton({}, { isAvailable: false });
            result.container.textContent!.should.equal('');
        });
    });
});
