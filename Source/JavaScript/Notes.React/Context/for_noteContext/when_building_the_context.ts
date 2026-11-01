// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { SliceLocation } from '@cratis/arc/reflection';
import { orderSnapshot } from '../../given/aNoteContext.js';
import { buildNoteContext } from '../noteContext.js';

describe('when building the context', () => {
    const correlationId = Guid.create();
    const context = buildNoteContext({
        snapshot: orderSnapshot({ contributions: { modelReferences: { eventModel: 'orders-v3', slice: 42 }, other: 'ignored' } }),
        route: '/orders/42',
        correlationId
    });

    it('should have the focused location', () => context.location.should.include({ module: 'Orders', feature: 'Editing', slice: 'Change order' }));
    it('should have every active slice', () => context.activeSlices.map(slice => slice.slice).should.deep.equal(['Change order', 'Order list']));
    it('should have the screen', () => context.screen.should.include({ screenId: 'order-details', screenName: 'Order details', screenPath: '/orders/42' }));
    it('should have the route', () => context.route.should.equal('/orders/42'));
    it('should have no dialog', () => context.dialog.dialogTitle.should.equal(''));
    it('should have the model references the host contributed, as text', () => context.modelReferences.should.deep.equal({ eventModel: 'orders-v3', slice: '42' }));
    it('should have the correlation id', () => context.correlationId.should.equal(correlationId));
});

describe('when building the context in a dialog', () => {
    const context = buildNoteContext({
        snapshot: orderSnapshot({ dialogs: [{ id: 'd1', title: 'Change order', location: new SliceLocation('Orders', 'Editing', 'Change order dialog') }] }),
        route: '/orders/42',
        correlationId: Guid.create()
    });

    it('should name the dialog', () => context.dialog.dialogTitle.should.equal('Change order'));
    it('should place the dialog', () => context.dialog.location.slice.should.equal('Change order dialog'));
    it('should focus the location of the dialog', () => context.location.slice.should.equal('Change order dialog'));
});

describe('when building the context for a dialog given by the button', () => {
    const context = buildNoteContext({
        snapshot: orderSnapshot(),
        route: '/orders/42',
        correlationId: Guid.create(),
        dialog: { title: 'Pay', location: { module: 'Billing', feature: 'Invoices', slice: 'Pay' } }
    });

    it('should use the dialog the button is in', () => context.dialog.dialogTitle.should.equal('Pay'));
    it('should focus the location of that dialog', () => context.location.module.should.equal('Billing'));
});

describe('when building the context without a screen or contributions', () => {
    const context = buildNoteContext({
        snapshot: orderSnapshot({ screen: { id: '', name: '', path: '' }, contributions: { modelReferences: 'not an object' } }),
        route: '/',
        correlationId: Guid.create()
    });

    it('should have empty model references', () => Object.keys(context.modelReferences).should.be.empty);
    it('should have an empty screen id', () => context.screen.screenId.should.equal(''));
});
