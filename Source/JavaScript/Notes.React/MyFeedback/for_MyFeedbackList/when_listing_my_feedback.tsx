// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { MyNote, NoteStatus, NoteTarget } from '@cratis/showrunner.notes';
import { cleanup, fireEvent, render } from '@testing-library/react';
import sinon from 'sinon';
import { defaultNotesLabels } from '../../Labels/defaultNotesLabels.js';
import { MyFeedbackList } from '../MyFeedbackList.js';

const note = (overrides: Partial<MyNote> = {}): MyNote => Object.assign(new MyNote(), {
    id: Guid.create(),
    text: 'The save button is hard to find',
    status: NoteStatus.given,
    statusDetail: '',
    target: NoteTarget.context,
    location: { module: 'Orders', feature: 'Editing', slice: 'Change order' },
    screen: { screenId: '', screenName: 'Order details', screenPath: '/orders/42' },
    slices: [],
    component: '',
    propertyChanges: [],
    lastActivityAt: new Date('2026-11-01T10:00:00Z'),
    ...overrides
});

describe('when listing my feedback', () => {
    afterEach(() => cleanup());

    describe('and there is none', () => {
        it('should say so', () => render(<MyFeedbackList notes={[]} labels={defaultNotesLabels} onWithdraw={sinon.stub()} />).getByText('You have not given any feedback yet.').textContent!.should.contain('feedback'));
    });

    describe('and notes arrive from the receiving side', () => {
        const given = note();
        const other = note({ text: 'Praise for the list', lastActivityAt: new Date('2026-10-01T10:00:00Z') });
        let onWithdraw: sinon.SinonStub;
        let result: ReturnType<typeof render>;

        beforeEach(() => {
            onWithdraw = sinon.stub();
            result = render(<MyFeedbackList notes={[other, given]} labels={defaultNotesLabels} onWithdraw={onWithdraw} />);
        });

        it('should list the most recent first', () => result.getAllByRole('listitem').map(item => item.querySelector('p')!.textContent).should.deep.equal(['The save button is hard to find', 'Praise for the list']));
        it('should say where each stands', () => result.getAllByText('Given').length.should.equal(2));

        it('should show an acknowledgement when it arrives', () => {
            result.rerender(<MyFeedbackList notes={[other, note({ ...given, id: given.id, status: NoteStatus.acknowledged })]} labels={defaultNotesLabels} onWithdraw={onWithdraw} />);
            result.getByText('Acknowledged').textContent!.should.equal('Acknowledged');
        });

        it('should show the version that addresses a note, and no way to withdraw it', () => {
            result.rerender(<MyFeedbackList notes={[note({ id: given.id, status: NoteStatus.addressed, statusDetail: '2.4.0' })]} labels={defaultNotesLabels} onWithdraw={onWithdraw} />);
            const text = result.getByText('Addressed in 2.4.0').textContent;
            (text === 'Addressed in 2.4.0' && result.queryByRole('button', { name: /Withdraw/ }) === null).should.equal(true);
        });

        it('should show the reason a note is declined', () => {
            result.rerender(<MyFeedbackList notes={[note({ id: given.id, status: NoteStatus.declined, statusDetail: 'Works as designed' })]} labels={defaultNotesLabels} onWithdraw={onWithdraw} />);
            result.getByText('Declined: Works as designed').textContent!.should.contain('Works as designed');
        });

        it('should withdraw the note chosen', () => {
            fireEvent.click(result.getByRole('button', { name: 'Withdraw the note: The save button is hard to find' }));
            onWithdraw.firstCall.args[0].should.equal(given);
        });

        it('should not withdraw a note that is being withdrawn', () => {
            result.rerender(<MyFeedbackList notes={[given]} labels={defaultNotesLabels} withdrawing={new Set([given.id.toString()])} onWithdraw={onWithdraw} />);
            (result.getByRole('button', { name: /Withdraw/ }) as HTMLButtonElement).disabled.should.equal(true);
        });
    });
});
