// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { MyNote, NoteStatus, NoteTarget } from '@cratis/showrunner.notes';
import { defaultNotesLabels } from '../../Labels/defaultNotesLabels.js';
import { canWithdraw, newestFirst, noteText, statusText, targetText } from '../noteStates.js';

const note = (overrides: Partial<MyNote> = {}): MyNote => Object.assign(new MyNote(), {
    text: 'The save button is hard to find',
    status: NoteStatus.given,
    statusDetail: '',
    target: NoteTarget.context,
    location: { module: 'Orders', feature: 'Editing', slice: 'Change order' },
    screen: { screenId: 'order-details', screenName: 'Order details', screenPath: '/orders/42' },
    slices: [],
    component: '',
    propertyChanges: [],
    lastActivityAt: new Date('2026-11-01T10:00:00Z'),
    ...overrides
});

describe('when showing where a note stands', () => {
    it('should say given', () => statusText(note(), defaultNotesLabels).should.equal('Given'));
    it('should say acknowledged', () => statusText(note({ status: NoteStatus.acknowledged }), defaultNotesLabels).should.equal('Acknowledged'));
    it('should say addressed in a version', () => statusText(note({ status: NoteStatus.addressed, statusDetail: '2.4.0' }), defaultNotesLabels).should.equal('Addressed in 2.4.0'));
    it('should say declined with the reason', () => statusText(note({ status: NoteStatus.declined, statusDetail: 'Works as designed' }), defaultNotesLabels).should.equal('Declined: Works as designed'));
    it('should say withdrawn', () => statusText(note({ status: NoteStatus.withdrawn }), defaultNotesLabels).should.equal('Withdrawn'));
});

describe('when showing what a note is about', () => {
    it('should say the location of what was on screen', () => targetText(note(), defaultNotesLabels).should.equal('What was on screen: Orders › Editing › Change order'));
    it('should say the slices', () => targetText(note({ target: NoteTarget.slices, slices: [{ module: 'Billing', feature: 'Invoices', slice: 'Pay' }, { module: 'Orders', feature: '', slice: '' }] }), defaultNotesLabels).should.equal('Slices: Billing › Invoices › Pay, Orders'));
    it('should say the screen', () => targetText(note({ target: NoteTarget.screen }), defaultNotesLabels).should.equal('Screen: Order details'));
    it('should say the component', () => targetText(note({ target: NoteTarget.component, component: 'Button' }), defaultNotesLabels).should.equal('Component Button · Orders › Editing › Change order'));
});

describe('when showing what a note says', () => {
    it('should show the text', () => noteText(note(), defaultNotesLabels).should.equal('The save button is hard to find'));
    it('should count the suggested changes of a note without text', () =>
        noteText(note({ text: '', propertyChanges: [{ property: 'variant', currentValue: 'solid', suggestedValue: 'outline' }, { property: 'size', currentValue: '', suggestedValue: 'small' }] }), defaultNotesLabels).should.equal('2 suggested property changes'));
});

describe('when deciding what can be withdrawn', () => {
    it('should withdraw a given note', () => canWithdraw({ status: NoteStatus.given }).should.be.true);
    it('should withdraw an acknowledged note', () => canWithdraw({ status: NoteStatus.acknowledged }).should.be.true);
    it('should not withdraw an addressed note', () => canWithdraw({ status: NoteStatus.addressed }).should.be.false);
    it('should not withdraw a declined note', () => canWithdraw({ status: NoteStatus.declined }).should.be.false);
    it('should not withdraw a withdrawn note', () => canWithdraw({ status: NoteStatus.withdrawn }).should.be.false);
    it('should put the most recent activity first', () => newestFirst([
        { lastActivityAt: new Date('2026-10-01') }, { lastActivityAt: new Date('2026-11-01') }
    ]).map(n => n.lastActivityAt.getMonth()).should.deep.equal([10, 9]));
});
