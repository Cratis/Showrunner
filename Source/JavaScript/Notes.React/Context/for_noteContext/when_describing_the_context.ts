// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { SliceLocation } from '@cratis/arc/reflection';
import { aNoteContext, orderSnapshot } from '../../given/aNoteContext.js';
import { defaultNotesLabels } from '../../Labels/defaultNotesLabels.js';
import { buildNoteContext, describeContext, describeScreen } from '../noteContext.js';

describe('when describing the context', () => {
    it('should say the location and the screen', () => describeContext(aNoteContext(), defaultNotesLabels).should.equal('Orders › Editing › Change order, on screen Order details'));

    it('should say the dialog', () => {
        const context = buildNoteContext({
            snapshot: orderSnapshot({ dialogs: [{ id: 'd1', title: 'Pay', location: new SliceLocation('Billing', 'Invoices', 'Pay') }] }),
            route: '/',
            correlationId: Guid.create()
        });
        describeContext(context, defaultNotesLabels).should.equal('Billing › Invoices › Pay, in the dialog Pay, on screen Order details');
    });

    it('should say only the screen when no slice is known', () =>
        describeContext(aNoteContext({ location: SliceLocation.empty, activeSlices: [] }), defaultNotesLabels).should.equal('Screen Order details'));

    it('should say the unknown location when nothing is known', () =>
        describeContext(aNoteContext({ location: SliceLocation.empty, screen: { id: '', name: '', path: '' } }), defaultNotesLabels).should.equal('Unknown location'));

    it('should say the screen of a screen note', () => describeScreen(aNoteContext(), defaultNotesLabels).should.equal('Screen Order details'));
});
